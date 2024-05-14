using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append(terminals);
            foreach (var item in nonterminals)
                sb.Append(string.Format("{0}: {1},",item.Key,item.Value));
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            //if(!(obj is SimplifiedWord))
            //    return false;
            return true;
            SimplifiedWord other = obj as SimplifiedWord;
            foreach(var nont in nonterminals)
                if(other.getNonterminal(nont.Key) != nont.Value)
                    return false;
            foreach (var nont in other.nonterminals)
                if (this.getNonterminal(nont.Key) != nont.Value)
                    return false;
            return true;
        }
        //public override int GetHashCode()
        //{
        //    return nonterminals.GetHashCode();
        //}
    }
}
