using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CA_utils;

namespace ShannonFanoCodes
{
    public class ShannonFanoCodes : ICompression
    {
        public Dictionary<char, string> CharToCode;
        public Dictionary<string, char> CodeToChar;

        private string _lastStr = String.Empty;
        private string _lastEncodedStr = String.Empty;

        public double CompressionRatio
        {
            get { return Math.Max(0.0, Math.Round(_lastStr.Length * 8.0 / _lastEncodedStr.Length, 5)); }
        }

        public string Encode(string text)
        {
            if (_lastStr != text && !(_lastStr == null && CharToCode != null))
            {
                BuildingCode(text);
                _lastStr = text;
            }

            var sb = new StringBuilder();
            foreach (var ch in text)
            { sb.Append(CharToCode[ch]); }
            _lastEncodedStr = sb.ToString();
            return sb.ToString();
        }

        public string Decode(string text)
        {
            if (CodeToChar == null) throw new Exception("Отсутствует словарь для расшифровки текста!");

            var sb = new StringBuilder();
            var code = new StringBuilder();

            int maxL = CodeToChar.Keys.Max(x => x.Length);

            foreach (var ch in text)
            {
                code.Append(ch);
                if (CodeToChar.ContainsKey(code.ToString()))
                {
                    sb.Append(CodeToChar[code.ToString()]);
                    code.Clear();
                }
                else if (code.Length >= maxL) throw new Exception("Некорректный словарь для расшифровки текста!");
            }
            return sb.ToString();
        }

        private void BuildingCode(string text)
        {
            CharToCode = new Dictionary<char, string>();
            CodeToChar = new Dictionary<string, char>();

            var frequencyDict = CA_Utils.GetFrequencyDict(text);

            if (frequencyDict.Keys.Count == 1)
            {
                char temp = frequencyDict.First(x => true).Key;
                CharToCode.Add(temp, "0");
                CodeToChar.Add("0", temp);
                return;
            }


            var tree = BinaryTree.Create(frequencyDict.ToList());
            tree.GetCharToCode(ref CharToCode);
            foreach (var pair in CharToCode)
            {
                CodeToChar.Add(pair.Value, pair.Key);
            }
        }

        public override string ToString()
        {
            return string.Join("\n", CharToCode.Select(x =>
            {
                switch (x.Key)
                {
                    case '\0': return $"'\\0' - {x.Value}";
                    case '\r': return $"'\\r' - {x.Value}";
                    case '\n': return $"'\\n' - {x.Value}";
                    default: return $"'{x.Key}' - {x.Value}";
                }
            }));
        }
    }
}
