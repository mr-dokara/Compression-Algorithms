using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CA_utils;

namespace RLEandBWT
{
    public class RLE : ICompression
    {
        public double CompressionRatio
        {
            get { return Math.Max(0.0, (_lastStr.Length * 8) / (_lastEncodedStr.Count(x => char.IsLetter(x)) * 8 * k)); }
        }

        public double AverageLength
        {
            get { return Math.Max(0.0, _lastEncodedStr.Length - 2.0); }
        }

        private int k;
        private string _lastStr = string.Empty;
        private string _lastEncodedStr = string.Empty;

        public string Encode(string text)
        {
            _lastStr = text;
            int count = 0;
            char lastChar = text[0];
            var sb = new StringBuilder();
            int max = 0;

            foreach (var ch in text)
            {
                if (ch == lastChar) count++;
                else
                {
                    if (char.IsDigit(lastChar)) sb.Append($"{count}--{lastChar}");
                    else sb.Append($"{count}{lastChar}");
                    if (count > max) max = count;
                    lastChar = ch;
                    count = 1;
                }
            }
            sb.Append($"{count}{lastChar}");

            k = 1;
            while (Math.Pow(2, k) < max) { k++; }

            _lastEncodedStr = sb.ToString();
            return sb.ToString();
        }

        public string Decode(string text)
        {
            var sb = new StringBuilder();
            var tempSb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i])) tempSb.Append(text[i]);
                else
                {
                    if (i < text.Length - 2 && text[i] == '-' && text[i + 1] == '-') i += 2;
                    
                    for (int j = 0; j < int.Parse(tempSb.ToString()); j++) 
                        sb.Append(text[i]);
                    tempSb.Clear();
                }
            }
            return sb.ToString();
        }
    }
}