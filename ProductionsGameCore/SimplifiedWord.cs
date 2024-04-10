using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace ProductionsGameCore
{
    public class SimplifiedWord
    {
        public int terminals { get; set; }
        public Dictionary<char, int> nonterminals = new Dictionary<char, int>();

        public SimplifiedWord()
        {
            terminals = 0;
        }

        public SimplifiedWord(string word)
        {
            terminals = 0;
            foreach (var c in word)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    if(!nonterminals.ContainsKey(c))
                        nonterminals[c] = 0;
                    nonterminals[c]++;
                }
                else terminals++;
            }
        }

        public SimplifiedWord(SimplifiedWord word) {
            terminals = word.terminals;
            foreach (var item in word.nonterminals)
                nonterminals.Add(item.Key,item.Value);
        }

        public void addNonterminal(char c,int count) {
            if (!nonterminals.ContainsKey(c))
                nonterminals[c] = 0;
            nonterminals[c]+=count;
        }

        public int getNonterminal(char c)
        {
            if (!nonterminals.ContainsKey(c))
                nonterminals[c] = 0;
            return nonterminals[c];
        }

        public IEnumerable<KeyValuePair<char,int>> getNonterminals() {
            return nonterminals.AsEnumerable();
        }

        public int getScore() {
            foreach (var nonterminal in nonterminals)
                if (nonterminal.Value > 0)
                    return 0;
            return 3 + terminals;
        }
    }
}
