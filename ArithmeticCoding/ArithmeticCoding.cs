using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CA_utils;

namespace ArithmeticCoding
{
    public class ArithmeticCoding : ICompression
    {
        public Dictionary<char, int> FreqDictionary
        {
            get { return _freqDictionary; }
            set
            {
                _freqDictionary = value;
                int sum = _freqDictionary.Sum(x => x.Value);
                _probabilities = new Dictionary<char, Fraction>();
                foreach (var pair in _freqDictionary)
                { _probabilities.Add(pair.Key, new Fraction(pair.Value, sum)); }
            }
        }

        private Dictionary<char, int> _freqDictionary;
        private Dictionary<char, Fraction> _probabilities;
        private string _lastStr = string.Empty;
        private string _lastEncodedStr = string.Empty;

        public double CompressionRatio
        {
            get { return Math.Max(0.0, (_lastStr.Length * 8) / (_lastEncodedStr.Length - 2.0) * 4); }
        }

        public double AverageLength
        {
            get { return Math.Max(0.0, _lastEncodedStr.Length - 2.0); }
        }

        public string Encode(string text)
        {
            text += '\0';
            if (_lastStr != text)
            {
                var _FreqDictionary = CA_Utils.GetFrequencyDict(text);
                FreqDictionary = _FreqDictionary;
                _probabilities = _probabilities.OrderByDescending(x => x.Value)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                _lastStr = text;
            }

            var segments = new Dictionary<char, KeyValuePair<Fraction, Fraction>>();
            Fraction l = new Fraction(0);
            foreach (var pair in _probabilities)
            {
                segments.Add(pair.Key, new KeyValuePair<Fraction, Fraction>(l, l + pair.Value));
                l = segments.Last().Value.Value;
            }

            Fraction start = new Fraction(0), end = new Fraction(1);
            foreach (var ch in text)
            {
                Fraction newStart = start + (end - start) * segments[ch].Key;
                Fraction newEnd = start + (end - start) * segments[ch].Value;
                start = newStart;
                end = newEnd;
            }

            _lastEncodedStr = ((start + end) / new Fraction(2)).ToString();
            return ((start + end) / new Fraction(2)).ToString();
        }

        public string Decode(string text)
        {
            var code = new Fraction(BigInteger.Parse(text.Split('/')[0]), BigInteger.Parse(text.Split('/')[1]));
            var sb = new StringBuilder();

            var segments = new Dictionary<char, KeyValuePair<Fraction, Fraction>>();
            Fraction l = new Fraction(0);
            foreach (var pair in _probabilities)
            {
                segments.Add(pair.Key, new KeyValuePair<Fraction, Fraction>(l, l + pair.Value));
                l = l + pair.Value;
            }

            bool flag = true;
            Fraction start = new Fraction(0), end = new Fraction(1);
            while (flag)
            {
                foreach (var pair in segments)
                {
                    if (code.CompareTo(segments[pair.Key].Key) >= 0  && code.CompareTo(segments[pair.Key].Value) <= 0)
                    {
                        if (pair.Key == '\0')
                        {
                            flag = false;
                            break;
                        }

                        sb.Append(pair.Key);
                        code = (code - segments[pair.Key].Key) / (segments[pair.Key].Value - segments[pair.Key].Key);
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return string.Join("\n", _probabilities.Select(x =>
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
