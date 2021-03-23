using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HuffmanCode
{
    public class PriorityQueue
    {
        private Queue<BinaryTree> queue;

        public int Count { get {return queue.Count;} }

        public PriorityQueue()
        {
            queue = new Queue<BinaryTree>();
        }

        public void Enqueue(BinaryTree binaryTree)
        {
            var list = queue.ToList();
            int k = 0;
            bool flag = false;
            while (k < list.Count)
            {
                if (list[k].Frequency >= binaryTree.Frequency)
                {
                    list.Insert(k, binaryTree);
                    flag = true;
                    break;
                }
                k++;
            }
            if (!flag) list.Add(binaryTree);
            
            queue.Clear();
            foreach (var pair in list)
            {
                queue.Enqueue(pair);
            }
        }

        public BinaryTree Dequeue()
        {
            return queue.Dequeue();
        }
    }
}