using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace ProductionsGameCore
{
    public class SimplifiedWord
    {
        public int terminals { get; set; }
        public int this[int i]{ 
            get => nonterminals[i];
            set => nonterminals[i] = value; 
        }
        public int NonterminalsCount { get => nonterminals.Length; }
        internal int[] nonterminals;

        internal SimplifiedWord()
        {
            terminals = 0;
        }

        public SimplifiedWord(SimplifiedWord word)
        {
            terminals = word.terminals;
            nonterminals = new int[word.NonterminalsCount];
            for (int i = 0; i < word.NonterminalsCount; i++)
                nonterminals[i] = word.nonterminals[i];
        }

        public void addNonterminal(int index,int count) {
            nonterminals[index]+=count;
        }

        public int getNonterminal(int c)
        {
            return nonterminals[c];
        }

        public int[] getNonterminals() {
            return nonterminals;
        }

        public int getScore() {
            foreach (var nonterminal in nonterminals)
                if (nonterminal > 0)
                    return 0;
            return 3 + terminals;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in nonterminals)
                sb.Append(string.Format("{0} ",item));
            return sb.ToString();
        }
    }
}
