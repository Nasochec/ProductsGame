using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace ProductsGame
{
    public class Production
    {
        public char Left
        {
            get;
            private set;
        }
        private List<string> right = new List<string>();

        Production(char left, List<string> right)
        {
            this.Left = left;
            right.ForEach(x => this.right.Add(x));
        }

        Production(char left, params string[] right)
        {
            this.Left = left;
            foreach (string tmp in right)
                this.right.Add(tmp);
        }

        public int RightSize
        {
            get => right.Count;
        }

        public string getRightAt(int index)
        {
            if (index >= 0 && index < RightSize)
                return right[index];
            throw new IndexOutOfRangeException(
                String.Format("Index {0} was out of range [0,{1})", index, RightSize)
                );
        }

    }
}
