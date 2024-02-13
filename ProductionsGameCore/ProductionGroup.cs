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
            if (!(left >= 'A' && left <= 'Z'))
                throw new ArgumentException("Letter in left part of prosuction must be capital english letter.");
            this.Left = left;
            if (right.Count == 0)
                throw new ArgumentException("In each production group must at least one production.");
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
                String.Format("Index {0} was outside of [0,{1}).", index, RightSize)
                );
        }

        public IEnumerable<string> getRights()
        {
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

        public static ProductionGroup fromString(string s)
        {
            StringBuilder sb = new StringBuilder();
            char? left = null;
            List<string> right = new List<string>();
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] == '|')
                {
                    if (left == null)
                        throw new ArgumentException("Left part of production is not specified.");
                    right.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                if (s[i] == '-' && i + 1 < s.Length && s[i + 1] == '>')//found ->
                {
                    if (left == null)
                        if (sb.Length == 1 && sb[0] >= 'A' && sb[0] <= 'Z')//check that Neterminal in left
                        {
                            left = sb[0];
                            sb.Clear();
                            i++;//skip >
                            continue;
                        }
                        else
                            throw new ArgumentException("Left part of production must be capital engliah letter.");
                }
                sb.Append(s[i]);
            }
            right.Add(sb.ToString());
            if (left == null)
                throw new ArgumentException("Left part of production is not specified.");
            return new ProductionGroup(left.Value, right);
        }
    }
}
