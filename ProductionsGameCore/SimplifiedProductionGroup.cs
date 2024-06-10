using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProductionsGameCore
{
    public class SimplifiedProductionGroup
    {
        public int Left { get; private set; }
        public int RightSize { get { return rights.Length; } }
        public SimplifiedWord this[int i] { get => rights[i]; internal set => rights[i] = value; }
        private SimplifiedWord[] rights;

        internal SimplifiedProductionGroup(int left,int rightSize)
        {
            this.Left = left;
            rights = new SimplifiedWord[rightSize];
        }

        public int getRightTerminalsAt(int index)
        {
            return rights[index].terminals;
        }

        public IEnumerable<SimplifiedWord> getRights()
        {
            return rights;
        }
    }
}
