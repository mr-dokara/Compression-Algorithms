using System;
using System.Runtime.Remoting.Messaging;
using CA_utils;
using System.Text;
using System.Text.RegularExpressions;

namespace LZ77Alg
{
    public class LZ77 : ICompression
    {
        private double _compressionRatio = 0, _averageLength = 0;
        private string _dictionary;
        public double CompressionRatio
        {
            get
            {
                if (_compressionRatio == 0)
                    throw new InvalidOperationException("Сначала выполни Encode.");
                return _compressionRatio;
            }
            private set
            {
                _compressionRatio = value;
            }
        }

        public double AverageLength
        {
            get
            {
                if (_averageLength== 0)
                    throw new InvalidOperationException("Сначала выполни Encode.");
                return _averageLength;
            }
            private set
            {
                _averageLength = value;
            }
        }

        public string Encode(string text)
        {
            var result = new StringBuilder();
            text = RemoveRNinText(text);
            var buffer = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                var substr = new StringBuilder();
                substr.Append(text[i]);
                while (buffer.ToString().Contains(substr.ToString()))
                {
                    if (i < text.Length-1)
                    {
                        i++;
                        substr.Append(text[i]);
                    }
                    else
                    {
                        break;
                    }
                }

                if (substr.Length > 1)
                {
                    substr.Remove(substr.Length - 1, 1);
                    result.AppendLine($"{buffer.ToString().IndexOf(substr.ToString()) + 1} {substr.Length} {text[i]}");
                    buffer.Append(substr + text[i].ToString());
                }
                else
                {
                    result.AppendLine($"0 0 {text[i]}");
                    buffer.Append(text[i]);
                }
            }
            CompressionRatio = (double)text.Length * 8 / GetResSize(result.ToString());
            AverageLength = (double)result.Length / text.Length;
            _dictionary = result.ToString();
            return result.ToString();
        }

        public string Decode(string text)
        {
            var result = new StringBuilder();
            var regex = new Regex(@"\d+ \d+ .");
            foreach (var match in regex.Matches(text))
            {
                var line = match.ToString().Split(' ');
                var position = int.Parse(line[0]);
                char symbol;
                if (line[2].Length == 1)
                    symbol = line[2][0];
                else
                    symbol = ' ';
                if (position != 0)
                {
                    var length = int.Parse(line[1]);
                    result.Append(result.ToString().Substring(position - 1, length));
                }
                result.Append(symbol);
            }
            return result.ToString();
        }

        private string RemoveRNinText(string text)
        {
            return text.Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private int GetResSize(string result)
        {
            var regex = new Regex(@"\d+ \d+ .");
            int count = 0;
            foreach (var match in regex.Matches(result))
            {
                count += (match.ToString().Length - 3) * 4 + 8;
            }
            return count;
        }
        public override string ToString()
        {
            return _dictionary;
        }
    }
}
