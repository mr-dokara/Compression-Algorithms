using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CA_utils;
using Microsoft.Win32;
using Newtonsoft.Json;


namespace WPF
{
    public partial class MainWindow : Window
    {
        private HashSet<ICompression> _AlgorithmsUsed = new HashSet<ICompression>();
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
                    ICompression ret = _AlgorithmsUsed.FirstOrDefault(x => x is HuffmanCode.HuffmanCode);
                    if (ret != null) return ret;
                    ret = new HuffmanCode.HuffmanCode();
                    _AlgorithmsUsed.Add(ret);
                    return ret;

                case "Коды Фано-Шеннона":
                    ICompression rets = _AlgorithmsUsed.FirstOrDefault(x => x is ShannonFanoCodes.ShannonFanoCodes);
                    if (rets != null) return rets;
                    rets = new ShannonFanoCodes.ShannonFanoCodes();
                    _AlgorithmsUsed.Add(rets);
                    return rets;
            }
            return null;
        }

        private void SaveFile(string text, bool dialog=false)
        {
            string path = $"{DateTime.Now.Millisecond}.ef";
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

        private void MenuItems_Click_Algorithms(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ICompression algorithm = GetAlgorithm((item.Parent as MenuItem).Header.ToString());

            if (item.Header.ToString()[0] == 'З')
            {
                _LastAlgorithm = algorithm;
                tbEncodedText.Text = algorithm.Encode(tbText.Text);
                tbCharToCode.Text = algorithm.ToString();

                btnStatistics.IsEnabled = true;
                
            }
            else if (item.Header.ToString()[0] == 'Д')
            {
                string decodedText = string.Empty;
                try
                {
                    decodedText = algorithm.Decode(tbEncodedText.Text);
                    tbText.Text = decodedText;
                    btnStatistics.IsEnabled = true;
                }
                catch
                {
                    _AlgorithmsUsed.Remove(algorithm);
                    algorithm = GetAlgorithm((item.Parent as MenuItem).Header.ToString());
                    try
                    {
                        decodedText = algorithm.Decode(tbEncodedText.Text);
                        tbText.Text = decodedText;
                        btnStatistics.IsEnabled = true;
                    }
                    catch { MessageBox.Show("Невозможно раскодировать файл выбранным алгоритмом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
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
                    _AlgorithmsUsed.Remove(_AlgorithmsUsed.FirstOrDefault(x => x.GetType() == ef.AlgorithmType));
                    _AlgorithmsUsed.Add(_LastAlgorithm);
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
