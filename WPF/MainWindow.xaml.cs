using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CA_utils;
using Microsoft.Expression.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;


namespace WPF
{
    public partial class MainWindow : Window
    {
        private object locker = new object();
        private ICompression _LastAlgorithm;

        public MainWindow()
        {
            InitializeComponent();

        }

        private ICompression GetAlgorithm(string name)
        {
            switch (name)
            {
                case "Коды Хаффмана":
                    if (_LastAlgorithm is HuffmanCode.HuffmanCode) return _LastAlgorithm;
                    return new HuffmanCode.HuffmanCode();

                case "Коды Фано-Шеннона":
                    if (_LastAlgorithm is ShannonFanoCodes.ShannonFanoCodes) return _LastAlgorithm;
                    return new ShannonFanoCodes.ShannonFanoCodes();
            }
            return null;
        }

        private void SaveFile(string text, bool dialog = false)
        {
            string path = $"{DateTime.Now.ToString("d")}.ef";
            if (dialog)
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "Encoded files|*.ef";
                sfd.FileName = "New Encoded File";
                if (sfd.ShowDialog() == true) path = sfd.FileName;
            }

            var ef = new EF { AlgorithmType = _LastAlgorithm.GetType(), Algorithm = JsonConvert.SerializeObject(_LastAlgorithm), EncodedText = tbEncodedText.Text };

            using (var sw = new StreamWriter(path))
            { sw.WriteLine(JsonConvert.SerializeObject(ef)); }
        }

        private void Clear()
        {
            btnStatistics.IsEnabled = false;
            tbText.Text = string.Empty;
            tbEncodedText.Text = string.Empty;
            tbCharToCode.Text = string.Empty;
            tbFilePath.Text = "не выбран";
        }

        private async void StartLoadingAnimationAsync(CancellationToken token, bool infinite = false)
        {
            LoadingGrid.Visibility = Visibility.Visible;
            await Task.Run(() =>
            {
                do
                {
                    for (int i = 0; i < 360; i += 10)
                    {
                        arcLoading.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            arcLoading.StartAngle = i;
                            arcLoading.EndAngle = i + 60;
                        }));

                        Task.Delay(5).Wait();
                    }
                } while (infinite && !token.IsCancellationRequested);
            }, token);
        }

        private async void EncodeAsync(ICompression algorithm, string text, CancellationTokenSource src)
        {
            await Task.Run(() => algorithm.Encode(text)).ContinueWith(task =>
            {
                _LastAlgorithm = algorithm;
                Dispatcher.BeginInvoke(new Action((() =>
                {
                    tbEncodedText.Text = task.Result;
                    tbCharToCode.Text = algorithm.ToString();
                    btnStatistics.IsEnabled = true;
                    src.Cancel();
                    LoadingGrid.Visibility = Visibility.Hidden;
                })));
                //tbEncodedText.Dispatcher.BeginInvoke(new Action(() => tbEncodedText.Text = task.Result));
                //tbCharToCode.Dispatcher.BeginInvoke(new Action(() => tbCharToCode.Text = algorithm.ToString()));
                //btnStatistics.Dispatcher.BeginInvoke(new Action(() => btnStatistics.IsEnabled = true));
                //src.Cancel();
                //LoadingGrid.Dispatcher.BeginInvoke(new Action(() => LoadingGrid.Visibility = Visibility.Hidden));
            });
        }

        private async void DecodeAsync(ICompression algorithm, string enText, CancellationTokenSource src)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var decodedText = algorithm.Decode(enText);
                    lock (locker)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            tbText.Text = decodedText;
                            btnStatistics.IsEnabled = true;
                            src.Cancel();
                        });
                    }
                }
                catch
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Невозможно раскодировать файл выбранным алгоритмом!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
                }
            });
        }

        private void MenuItems_Click_Algorithms(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ICompression algorithm = GetAlgorithm((item.Parent as MenuItem).Header.ToString());
            var text = tbText.Text;
            var enText = tbEncodedText.Text;
            var src = new CancellationTokenSource();

            if (item.Header.ToString()[0] == 'З')
            {
                StartLoadingAnimationAsync(src.Token, true);
                EncodeAsync(algorithm, text, src);
            }
            else if (item.Header.ToString()[0] == 'Д')
            {
                StartLoadingAnimationAsync(src.Token, true);
                DecodeAsync(algorithm, text, src);
            }
        }

        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Text files|*.txt";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                Clear();
                var src = new CancellationTokenSource();
                tbFilePath.Text = ofd.FileName;
                tbText.Text = File.ReadAllText(ofd.FileName);
            }
        }

        private void MenuItem_Click_OpenEF(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Encoded files|*.ef";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                Clear();
                var ef = JsonConvert.DeserializeObject<EF>(File.ReadAllText(ofd.FileName));
                _LastAlgorithm = JsonConvert.DeserializeObject(ef.Algorithm, ef.AlgorithmType) as ICompression;
                tbFilePath.Text = ofd.FileName + $" (Алгоритм: {ef.AlgorithmType.Namespace})";
                tbEncodedText.Text = ef.EncodedText;
                tbCharToCode.Text = _LastAlgorithm.ToString();
            }
        }

        private void MenuItem_Click_Clear(object sender, RoutedEventArgs e)
            => Clear();

        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
            => SaveFile(tbEncodedText.Text);

        private void MenuItem_Click_SaveAs(object sender, RoutedEventArgs e)
            => SaveFile(tbEncodedText.Text, true);

        private void Button_Click_Statistics(object sender, RoutedEventArgs e)
        {
            var stat = new Statistics();
            stat.TextLength.Content = tbText.Text.Length;
            stat.EncodedTextLength.Content = tbEncodedText.Text.Length;
            stat.CompRatio.Content = _LastAlgorithm.CompressionRatio;
            stat.CompRatioArc.EndAngle = Math.Max(0, 100.0 / _LastAlgorithm.CompressionRatio) * 3.6;
            stat.CompRatioText.Content = $"в {_LastAlgorithm.CompressionRatio} раз";
            stat.ShowDialog();
        }
    }
}
