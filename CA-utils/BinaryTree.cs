using System.Collections.Generic;
using System.Text;

namespace CA_utils
{
    public class BinaryTree
    {
        public BinaryTree Root;
        public BinaryTree LeftChild, RightChild;

        public int Frequency;
        public char Char;

        public int CountNodes = 1;

        public static BinaryTree Create(char ch, int freq)
        {
            return new BinaryTree { Char = ch, Frequency = freq };
        }

        public static BinaryTree Create(BinaryTree binaryTree1, BinaryTree binaryTree2)
        {
            var temp = new BinaryTree
            {
                Frequency = binaryTree1.Frequency + binaryTree2.Frequency,
                LeftChild = binaryTree1,
                RightChild = binaryTree2,
                CountNodes = binaryTree1.CountNodes + binaryTree2.CountNodes
            };
            binaryTree1.Root = temp;
            binaryTree2.Root = temp;
            return temp;
        }

        public static BinaryTree Create(List<KeyValuePair<char, int>> freq)
        {
            if (freq.Count == 1) return Create(freq[0].Key, freq[0].Value);

            var leftList = new List<KeyValuePair<char, int>>();
            var rightList = new List<KeyValuePair<char, int>>();
            int left = 0, right = 0;

            foreach (var pair in freq)
            {
                if (left < right)
                {
                    leftList.Add(pair);
                    left += pair.Value;
                }
                else
                {
                    rightList.Add(pair);
                    right += pair.Value;
                }
            }

            return new BinaryTree {LeftChild = Create(leftList), RightChild = Create(rightList)};
        }

        public void GetCharToCode(ref Dictionary<char, string> charToCode, StringBuilder sb=null)
        {
            if (sb == null) sb = new StringBuilder();

            if (LeftChild != null)
            {
                sb.Append("0");
                LeftChild.GetCharToCode(ref charToCode, sb);
            }
            if (RightChild != null)
            {
                sb.Append("1");
                RightChild.GetCharToCode(ref charToCode, sb);
            }

            if (LeftChild == null && RightChild == null)
                charToCode.Add(Char, sb.ToString());
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
        }
    }
}