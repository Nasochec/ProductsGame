﻿using System.Collections.Generic;

namespace StrategyUtilities
{
    public class SimplifiedWord
    {
        public int terminals { get; set; }
        public Dictionary<char, int> neterminalsCount = new Dictionary<char, int>();
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
                    if(!neterminalsCount.ContainsKey(c))
                        neterminalsCount[c] = 0;
                    neterminalsCount[c]++;
                }
                else terminals++;
            }
        }

        public SimplifiedWord(SimplifiedWord word) {
            terminals = word.terminals;
            foreach (var item in word.neterminalsCount)
                neterminalsCount.Add(item.Key,item.Value);
        }

        public void addNeterminal(char c,int count) {
            if (!neterminalsCount.ContainsKey(c))
                neterminalsCount[c] = 0;
            neterminalsCount[c]+=count;
        }

        public int getNeterminal(char c)
        {
            if (!neterminalsCount.ContainsKey(c))
                neterminalsCount[c] = 0;
            return neterminalsCount[c];
        }
    }
}
