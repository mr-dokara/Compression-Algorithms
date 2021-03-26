using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CA_utils;
using HuffmanCode;
using Microsoft.Expression.Shapes;
using Microsoft.Win32;


namespace WPF
{
    public partial class MainWindow : Window
    {
        private HashSet<ICompression> AlgorithmsUsed = new HashSet<ICompression>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ICompression algorithm = GetAlgorithm((item.Parent as MenuItem).Header.ToString());

            if (item.Header.ToString()[0] == 'З')
            {
                tbEncodedText.Text = algorithm.Encode(tbText.Text);
                tbCharToCode.Text = algorithm.ToString();
                double ratio = Math.Max(0.0, Math.Round(100.0 - (tbEncodedText.Text.Length / (tbText.Text.Length * 0.08)), 1));
                ArcCompRatio.EndAngle = ratio * 3.6;
                lbCompRatio.Content = $"~{ratio}%";
            }
            else if (item.Header.ToString()[0] == 'Д')
            {
                string decodedText;
                try { decodedText = algorithm.Decode(tbEncodedText.Text); }
                catch
                {
                    AlgorithmsUsed.Clear();
                    algorithm = GetAlgorithm((item.Parent as MenuItem).Header.ToString());
                    decodedText = algorithm.Decode(tbEncodedText.Text);
                }
                

                tbText.Text = decodedText;
            }
        }

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Text files|*.txt";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                tbFilePath.Text = ofd.FileName;
                tbText.Text = File.ReadAllText(ofd.FileName);
            }
        }

        private ICompression GetAlgorithm(string name)
        {
            switch (name)
            {
                case "Коды Хаффмана":
                    ICompression ret = AlgorithmsUsed.FirstOrDefault(x => x is HuffmanCode.HuffmanCode);
                    if (ret != null) return ret;
                    ret = new HuffmanCode.HuffmanCode();
                    AlgorithmsUsed.Add(ret);
                    return ret;

                case "Коды Фано-Шеннона":
                    ICompression rets = AlgorithmsUsed.FirstOrDefault(x => x is ShannonFanoCodes.ShannonFanoCodes);
                    if (rets != null) return rets;
                    rets = new ShannonFanoCodes.ShannonFanoCodes();
                    AlgorithmsUsed.Add(rets);
                    return rets;
            }
            return null;
        }

        private void MenuItem_Clear(object sender, RoutedEventArgs e)
        {
            tbText.Text = string.Empty;
            tbEncodedText.Text = string.Empty;
            tbCharToCode.Text = string.Empty;
            tbFilePath.Text = "не выбран";
        }
    }



    public class BindableMenuItem
    {
        public string Name { get; set; }
        public BindableMenuItem[] Children { get; set; }
        public ICommand Command { get; set; }
    }
}
