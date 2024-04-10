using System.Collections.Generic;

namespace ProductionsGameCore
{
    public class SimplifiedProductionGroup
    {
        public char Left { get; private set; }
        public int RightSize { get { return rights.Count; } }
        public List<SimplifiedWord> rights;

        public SimplifiedProductionGroup(ProductionGroup group)
        {
            this.Left = group.Left;
            rights = new List<SimplifiedWord>();
            for (int i = 0; i < group.RightSize; ++i)
                rights.Add(new SimplifiedWord(group.getRightAt(i)));
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
