using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    [Serializable]
    public class ProductionGroup : ISerializable
    {
        public char Left
        {
            get;
            private set;
        }
        private List<string> right = new List<string>();

        public ProductionGroup(char left, List<string> right)
        {
            this.Left = left;
            right.ForEach(x => this.right.Add(x));
        }

        public ProductionGroup(char left, params string[] right)
        {
            this.Left = left;
            foreach (string tmp in right)
                this.right.Add(tmp);
        }

        public ProductionGroup(SerializationInfo info, StreamingContext context)
        {
            Left = (char)info.GetValue("left", typeof(char));
            int size = (int)info.GetValue("rightSize", typeof(int));
            for (int i = 0; i < size; ++i)
                right.Add((string)info.GetValue("right" + i, typeof(string)));
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

        public IEnumerable<string> getRights() { 
            return right.AsEnumerable();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("left", Left, typeof(char));
            info.AddValue("rightSize", RightSize, typeof(int));
            for (int i = 0; i < RightSize; ++i)
                info.AddValue("right" + i, right[i], typeof(string));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Left + "->");
            if (right.Count != 0)
                sb.Append(right[0].ToString());
            for (int index = 1; index < right.Count; ++index)
            {
                sb.Append("|" + right[index].ToString());
            }
            return sb.ToString();
        }

        //public static ProductionGroup fromString(string s)
        //{
        //    string[] splitted = s.Split(' ');


        //    string[] splitted = s.Split(' ');
        //    if (splitted[0].Length != 1 || !(splitted[0][0] >= 'A' && splitted[0][0] >= 'Z'))
        //        throw new ArgumentException("String was in wrong format.");
        //    char left = splitted[0][0];
        //    char right = splitted;
        //    ProductionGroup production = new ProductionGroup();
        //}
    }
}
