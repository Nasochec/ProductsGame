using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyUtilities
{

    /// <summary>
    /// Упрощённые группы продукций, где правая часть продукции упрощается до количества в ней терминалов и множества содержащихся в ней нетерминалов.
    /// </summary>
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
    }
}
