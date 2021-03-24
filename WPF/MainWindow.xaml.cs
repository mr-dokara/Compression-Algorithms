using System;
using System.Collections.Generic;
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


namespace WPF
{
    public partial class MainWindow : Window
    {
        private HashSet<ICompression> AlgorithmsUsed = new HashSet<ICompression>();
        private string lastEncodedText;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ICompression algorithm = GetAlgorithm((item.Parent as MenuItem).Header.ToString());

            if (item.Header.ToString()[0] == 'З') MessageBox.Show(lastEncodedText = algorithm.Encode("sdfgdfghcvx;,bmsdel;kshl;kdfngvzxxdfgsdbklajsbdvkjbdesoirlbslkdjf"));
            else if (item.Header.ToString()[0] == 'Д') MessageBox.Show(algorithm.Decode(lastEncodedText));
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
            }
            return null;
        }
    }

    

    public class BindableMenuItem
    {
        public string Name { get; set; }
        public BindableMenuItem[] Children { get; set; }
        public ICommand Command { get; set; }
    }
}
