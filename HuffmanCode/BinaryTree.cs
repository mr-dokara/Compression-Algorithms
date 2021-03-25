using System;
using System.Collections.Generic;
using System.Text;

namespace HuffmanCode
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

        public void GetCharToCode(StringBuilder sb, ref Dictionary<char, string> charToCode)
        {
            if (LeftChild != null)
            {
                sb.Append("0");
                LeftChild.GetCharToCode(sb, ref charToCode);
            }
            if (RightChild != null)
            {
                sb.Append("1");
                RightChild.GetCharToCode(sb, ref charToCode);
            }

            if (LeftChild == null && RightChild == null)
                charToCode.Add(Char, sb.ToString());
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
        }
    }
}