using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CA_utils;

namespace HuffmanCode
{
    public class HuffmanCode : ICompression
    {
        private Dictionary<char, string> _CharToCode;
        private Dictionary<string, char> _CodeToChar;

        public string Encode(string text)
        {
            BuildingCode(text);

            var sb = new StringBuilder();
            foreach (var ch in text)
            { sb.Append(_CharToCode[ch]); }
            return sb.ToString();
        }

        public string Decode(string text)
        {
            if (_CodeToChar == null) throw new Exception("Отсутствует словарь для расшифровки текста!");

            var sb = new StringBuilder();
            var code = new StringBuilder();

            int maxL = _CodeToChar.Keys.Max(x => x.Length);

            foreach (var ch in text)
            {
                code.Append(ch);
                if (_CodeToChar.ContainsKey(code.ToString()))
                {
                    sb.Append(_CodeToChar[code.ToString()]);
                    code.Clear();
                }
                else if (code.Length >= maxL) throw new Exception("Некорректный словарь для расшифровки текста!");
            }
            return sb.ToString();
        }

        private void BuildingCode(string text)
        {
            _CharToCode = new Dictionary<char, string>();
            _CodeToChar = new Dictionary<string, char>();

            var frequencyDict = CA_Utils.GetFrequencyDict(text);

            if (frequencyDict.Keys.Count == 1)
            {
                char temp = frequencyDict.First(x => true).Key;
                _CharToCode.Add(temp, "0");
                _CodeToChar.Add("0", temp);
                return;
            }

            PriorityQueue queue = new PriorityQueue();
            foreach (var pair in frequencyDict)
            {
                queue.Enqueue(BinaryTree.Create(pair.Key, pair.Value));
            }

            while (queue.Count > 1)
            {
                var binTree = BinaryTree.Create(queue.Dequeue(), queue.Dequeue());
                queue.Enqueue(binTree);
            }

            var binaryTree = queue.Dequeue();
            binaryTree.GetCharToCode(new StringBuilder(), ref _CharToCode);
            foreach (var pair in _CharToCode)
            {
                _CodeToChar.Add(pair.Value, pair.Key);
            }
        }

        string ICompression.GetCharToCode()
        {
            return string.Join("\n", _CharToCode.Select(x =>
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
