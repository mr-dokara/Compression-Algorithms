using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CA_utils;

namespace RLEandBWT
{
    public class BWT : ICompression
    {
        public double CompressionRatio
        {
            get { return Math.Max(0.0, (_lastStr.Length * 8) / (_lastEncodedStr.Length - 2.0) * 4); }
        }

        public double AverageLength
        {
            get { return Math.Max(0.0, _lastEncodedStr.Length - 2.0); }
        }

        private string _lastStr = string.Empty;
        private string _lastEncodedStr = string.Empty;
        private List<string> cicleAr;

        public string Encode(string text)
        {
            text += '$';
            _lastStr = text;
            cicleAr = new List<string>();

            for (int i = 0; i < text.Length - 1; i++)
            {
                cicleAr.Add(text);
                var temp = text.Substring(1);
                text =  temp + text[0];
            }

            cicleAr.Sort();
            _lastEncodedStr = string.Join("", cicleAr.Select(x => x[x.Length - 1]).ToArray());
            return _lastEncodedStr;
        }

        public string Decode(string text)
        {
            var cicle = new List<string>();
            foreach (var ch in text)
            { cicle.Add(ch.ToString()); }
            cicle.Sort();

            for (int i = 1; i < text.Length; i++)
            {
                for (int j = 0; j < text.Length; j++)
                { cicle[j] += text[j]; }
                cicle.Sort();
            }

            var result = cicle.FirstOrDefault(x => x[x.Length - 1] == '$');
            return result.Substring(0, result.Length-1);
        }

        public override string ToString()
        {
            return string.Join("\n", cicleAr);
        }
    }
}