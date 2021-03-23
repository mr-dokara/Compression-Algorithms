using System.Collections.Generic;

namespace CA_utils
{
    public static class CA_Utils
    {
        public static Dictionary<char, int> GetFrequencyDict(string text)
        {
            var dict = new Dictionary<char, int>();
            foreach (var ch in text)
            {

                if (dict.ContainsKey(ch)) dict[ch]++;
                else dict.Add(ch, 1);
            }
            return dict;
        }
    }
}