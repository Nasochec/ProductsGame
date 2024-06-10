using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    public class Simplifier
    {
        Dictionary<char, int> letters = new Dictionary<char, int>();
        Dictionary<int, char> reverseLetters = new Dictionary<int,char>();


        public Simplifier(List<ProductionGroup> groups) { 
            foreach(var group in groups) {
                if (!letters.ContainsKey(group.Left))
                {
                    reverseLetters.Add(letters.Count, group.Left);
                    letters.Add(group.Left, letters.Count);

                }
                foreach (var right in group.getRights())
                    foreach (var letter in right)
                        if (letter >= 'A' && letter <= 'Z')
                            if (!letters.ContainsKey(letter))
                            {
                                reverseLetters.Add(letters.Count, letter);
                                letters.Add(letter, letters.Count);
                            }   
            }
        }


        public List<SimplifiedProductionGroup> ConvertProductions(List<ProductionGroup> groups)
        {
            List<SimplifiedProductionGroup> rez = new List<SimplifiedProductionGroup>();
            foreach (var group in groups)
            {
                SimplifiedProductionGroup prod = new SimplifiedProductionGroup(letters[group.Left],group.RightSize);
                for (int i=0;i<group.RightSize;++i)
                    prod[i] = this.ConvertWord(group.getRightAt(i));
                rez.Add(prod);
            }
            return rez;
        }

        public SimplifiedWord ConvertWord(string word) {
            SimplifiedWord sword = new SimplifiedWord();
            sword.nonterminals = new int[letters.Count];
            foreach (var letter in word) {
                if (letter >= 'A' && letter <= 'Z')
                    sword[letters[letter]] += 1;
                else
                    sword.terminals += 1;
            }
            return sword;
        }

        public int ConvertChar(char letter) => letters[letter];
        public char GetChar(int letterIndex) => reverseLetters[letterIndex];
    }
}
