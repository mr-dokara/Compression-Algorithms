using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private ICompression _LastAlgorithm;
        private bool resultInFile = false;

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
            string path = $"New Encoded File ({DateTime.Now.ToString("d")}).ef";
            if (dialog)
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "Encoded files|*.ef";
                sfd.FileName = $"New Encoded File ({DateTime.Now.ToString("d")}).ef";
                if (sfd.ShowDialog() == true) path = sfd.FileName;
            }

            var ef = new EF { AlgorithmType = _LastAlgorithm.GetType(), Algorithm = JsonConvert.SerializeObject(_LastAlgorithm), EncodedText = (resultInFile?File.ReadAllText("result.txt"):tbEncodedText.Text) };

            using (var sw = new StreamWriter(path))
            { sw.WriteLine(JsonConvert.SerializeObject(ef)); }
        }

        private void Clear()
        {
            if (last != null) last.IsEnabled = false;
            tbText.Text = string.Empty;
            tbEncodedText.Text = string.Empty;
            tbCharToCode.Text = string.Empty;
            tbFilePath.Text = "не выбран";
            tb1CompRatio.Text = "в ? раз";
            tb2CompRatio.Text = "?";
            tbAvrLength.Text = "?";
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
            await Task.Run(() =>
            {
                var result = algorithm.Encode(text);
                _LastAlgorithm = algorithm;

                Dispatcher.Invoke((() =>
                {
                    tbCharToCode.Text = algorithm.ToString();
                    tb1CompRatio.Text = $"в {Math.Round(algorithm.CompressionRatio, 2)} раз";
                    var anim = new DoubleAnimation { From = 360, To = arcCompRatio.EndAngle = (100.0 / algorithm.CompressionRatio) * 3.6, Duration = TimeSpan.FromSeconds(1) };
                    arcCompRatio.BeginAnimation(Arc.EndAngleProperty, anim);
                    tb2CompRatio.Text = Math.Round(algorithm.CompressionRatio, 5).ToString();
                    tbAvrLength.Text = Math.Round(algorithm.AverageLength, 5).ToString();
                    resultInFile = false;
                    src.Cancel();
                    LoadingGrid.Visibility = Visibility.Hidden;
                    if (result.Length <= Math.Pow(2, 22)) tbEncodedText.Text = result;
                    else
                    {
                        using (var sw = new StreamWriter("result.txt")) { sw.Write(result); }
                        if (MessageBox.Show(
                            "Закодированный текст слишком большой,\nрезультат сохранен в файл 'result.txt'\nПопытаться вывести результат в программе?",
                            "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes) tbEncodedText.Text = result;
                        else
                        {
                            resultInFile = true;
                            tbEncodedText.Text = "Закодированный текст слишком большой, результат сохранен в файл 'result.txt'. При следующей операции декодирования будет обработан файл вывода ('result.txt').";
                        }
                    }
                    
                }));
            });
        }

        private async void DecodeAsync(ICompression algorithm, string enText, CancellationTokenSource src)
        {
            await Task.Run(() =>
            {
                try
                {
                    var result = algorithm.Decode(enText);
                    _LastAlgorithm = algorithm;

                    Dispatcher.Invoke(() =>
                    {
                        tbCharToCode.Text = algorithm.ToString();
                        tb1CompRatio.Text = $"в {Math.Round(algorithm.CompressionRatio, 2)} раз";
                        var anim = new DoubleAnimation { From = 360, To = arcCompRatio.EndAngle = (100.0 / algorithm.CompressionRatio) * 3.6, Duration = TimeSpan.FromSeconds(1) };
                        arcCompRatio.BeginAnimation(Arc.EndAngleProperty, anim);
                        tb2CompRatio.Text = Math.Round(algorithm.CompressionRatio, 5).ToString();
                        tbAvrLength.Text = Math.Round(algorithm.AverageLength, 5).ToString();
                        tbText.Text = result;
                    });
                }
                catch
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Невозможно декодировать текст текущим словарём символ-код!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Clear();
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        src.Cancel();
                        LoadingGrid.Visibility = Visibility.Hidden;
                    });
                }
            });
        }

        private MenuItem last;
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
                if (last != null) last.IsEnabled = false;
                last = (item.Parent as MenuItem).Items.SourceCollection.Cast<MenuItem>().First(x => x.Header.ToString() == "Декодировать");
                last.IsEnabled = true;
            }
            else if (item.Header.ToString()[0] == 'Д')
            {
                StartLoadingAnimationAsync(src.Token, true);
                if (resultInFile) DecodeAsync(algorithm, File.ReadAllText("result.txt"), src);
                else DecodeAsync(algorithm, enText, src);
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

        private void MenuItem_OnClick(object sender, RoutedEventArgs e) 
            => Application.Current.Shutdown();
    }
}
