using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMoveStrategy
{
    internal class SuperSimplifiedWord
    {
        public Dictionary<char, int> CharToInt = new Dictionary<char, int>();
        public int[] neterminalsCount;
        public int terminals;

        public SuperSimplifiedWord(string word, List<SimplifiedProductionGroup> prods) {
            foreach (var group in prods) {
                if (!CharToInt.ContainsKey(group.Left))
                    CharToInt.Add(group.Left, CharToInt.Count);
                for (int i = 0; i < group.RightSize; i++) {
                    foreach (var neterminal in group.rights[i].neterminalsCount) {
                        if (!CharToInt.ContainsKey(neterminal.Key))
                            CharToInt.Add(neterminal.Key, CharToInt.Count);
                    }
                }
            }
            neterminalsCount=new int[CharToInt.Count];
            foreach (char c in word) {
                if (CharToInt.ContainsKey(c))
                    ++neterminalsCount[CharToInt[c]];
                else
                    ++terminals;
            }
        }

    }
}
