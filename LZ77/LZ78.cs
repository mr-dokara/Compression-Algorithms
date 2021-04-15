using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CA_utils;

namespace LZ78
{
    public class LZ78 : ICompression
    {

        private Dictionary<string, int> _encodeStringsDict;
        private Dictionary<int, string> _decodeStringsDict;

        private string text;
        private double averageLength = 0;
        private double compressionRatio = 0;

        public double CompressionRatio
        {
            get
            {
                if (compressionRatio == 0)
                    throw new InvalidOperationException("Сначала выполни Encode");
                return compressionRatio;
            }
            private set => compressionRatio = value;
        }

        public double AverageLength
        {
            get
            {
                if (averageLength == 0)
                    throw new InvalidOperationException("Сначала выполни Encode");
                return averageLength;
            }
            private set => averageLength = value;
        }

        public string Decode(string text)
        {
            _decodeStringsDict = new Dictionary<int, string>();
            var result = new StringBuilder();
            var regex = new Regex(@"\d+\s.");
            foreach (var match in regex.Matches(text))
            {
                var line = match.ToString();
                int i = 0;
                var numSB = new StringBuilder();
                while (char.IsDigit(line[i]))
                {
                    numSB.Append(line[i]);
                    i++;
                }
                var num = int.Parse(numSB.ToString());
                var symbol = line[line.Length - 1].ToString();
                if (num == 0)
                {
                    result.Append(symbol);
                    _decodeStringsDict.Add(_decodeStringsDict.Count + 1, symbol);
                }
                else
                {
                    var newline = _decodeStringsDict[num] + symbol;
                    result.Append(newline);
                    _decodeStringsDict.Add(_decodeStringsDict.Count + 1, newline);
                }
            }
            return result.ToString();
        }


        public string Encode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
               throw new ArgumentException("Нет текста.");
            }
            this.text = RemoveRNinText(text);
            _encodeStringsDict = new Dictionary<string, int>();
            var result = new StringBuilder();
            for (int i = 0; i < this.text.Length; i++)
            {
                var keyInDict = FindStringInDictionary(i);
                if (string.IsNullOrEmpty(keyInDict))
                {
                    result.AppendLine("0 " + this.text[i]);
                    _encodeStringsDict.Add(this.text[i].ToString(), _encodeStringsDict.Count + 1);
                }
                else
                {
                    i += keyInDict.Length;
                    if (i >= this.text.Length)
                        i = this.text.Length - 1;
                    result.AppendLine(_encodeStringsDict[keyInDict] + " " + this.text[i]);
                    if(!_encodeStringsDict.ContainsKey(keyInDict + this.text[i].ToString()))
                        _encodeStringsDict.Add(keyInDict + this.text[i].ToString(), _encodeStringsDict.Count + 1);
                }
            }

            CompressionRatio = (double)this.text.Length * 8 / EncodedDictSize(result.ToString());
            AverageLength = (double)result.Length / this.text.Length;
            return result.ToString();
        }

        private string FindStringInDictionary(int indexInText)
        {
            if (_encodeStringsDict.Count > 0)
            {
                var tempDict = new Dictionary<string, int>();
                foreach (var key in _encodeStringsDict.Keys)
                {
                    if (key.StartsWith(text[indexInText].ToString()))
                    {
                        tempDict.Add(key, _encodeStringsDict[key]);
                    }
                }

                if (tempDict.Count == 0)
                    return string.Empty;

                var lineToFind = new StringBuilder();

                lineToFind.Append(text[indexInText]);
                lineToFind.Append(text[indexInText + 1]);
                int index = indexInText + 2;
                while (tempDict.Count > 0)
                {
                    var tempDict2 = new Dictionary<string, int>();
                    foreach (var key in tempDict.Keys)
                    {
                        if (key.StartsWith(lineToFind.ToString()))
                        {
                            tempDict2.Add(key, _encodeStringsDict[key]);
                        }
                    }

                    if (tempDict2.Count == 0)
                    {
                        return tempDict.First().Key;
                    }
                    else
                    {
                        tempDict = tempDict2;
                    }
                    if (index < text.Length)
                        lineToFind.Append(text[index]);
                    else
                        return tempDict.First().Key;
                    index++;
                }

                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        private string RemoveRNinText(string text)
        {
            return text.Replace("\r", "\\r").Replace("\n","\\n");
        }

        private int EncodedDictSize(string encoded)
        {
            int result = 0;
            foreach (var sym in encoded)
            {
                if (char.IsDigit(sym))
                {
                    result += 4;
                }
                else
                {
                    result += 8;
                }
            }
            return result;
        }
    }
}
