using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShannonFanoCodes
{
    public class BinaryTree
    {
        public BinaryTree LeftChild, RightChild;
        public List<KeyValuePair<char, int>> List;
        public char Char;

        public static BinaryTree Create(List<KeyValuePair<char, int>> list)
        {
            if (list.Count == 1) return new BinaryTree {Char = list[0].Key};

            var left = new List<KeyValuePair<char, int>>();
            var right = new List<KeyValuePair<char, int>>();

            bool flag = true;
            foreach (var pair in list.OrderByDescending(x => x.Value))
            {
                if (flag) left.Add(pair);
                else right.Add(pair);
                flag = !flag;
            }

            var tree = new BinaryTree { List = list, LeftChild = Create(left), RightChild = Create(right) };
            return tree;
        }

        public void GetCharToCode(ref Dictionary<char, string> dict, StringBuilder sb = null)
        {
            if (sb == null) sb = new StringBuilder();

            if (LeftChild != null)
            {
                sb.Append("1");
                LeftChild.GetCharToCode(ref dict, sb);
            }

            if (RightChild != null)
            {
                sb.Append("0");
                RightChild.GetCharToCode(ref dict, sb);
            }
            if (LeftChild == null && RightChild == null) dict.Add(Char, sb.ToString());
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
        }
    }
}