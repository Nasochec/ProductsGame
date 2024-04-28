﻿using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyUtilities
{
    public static class StrategyUtilitiesClass
    {
        public static bool isHaveLetter(SimplifiedWord word, char c)
        {
            return word.nonterminals.ContainsKey(c) && word.getNonterminal(c) > 0;
        }

        public static List<int> findMatches(IEnumerable<SimplifiedWord> words, char c)
        {
            List<int> indexes = new List<int>();
            int index = 0;
            foreach (var word in words)
            {
                if (isHaveLetter(word, c)) indexes.Add(index);
                index++;
            }
            return indexes;
        }

        public static int isHaveLetter(string word, char c)
        {
            return word.IndexOf(c);
        }

        public static List<int> findMatches(IEnumerable<string> words, char c)
        {
            List<int> indexes = new List<int>();
            int index = 0;
            foreach (string word in words)
            {
                if (isHaveLetter(word, c) != -1) indexes.Add(index);
                index++;
            }
            return indexes;
        }


        public static void countMetric(List<SimplifiedProductionGroup> productions,
            RandomSettings rs,
            out double[] netMetric,
            out double[][] prodMetric
            )
        {
            netMetric = new double[productions.Count];
            prodMetric = new double[productions.Count][];
            int productionsCount = productions.Count;
            const double eps = 1e-9;

            //initialization
            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
            {
                netMetric[prodIndex] = -1;
                prodMetric[prodIndex] = new double[productions[prodIndex].RightSize];
                for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                    if (productions[prodIndex].rights[rightIndex].nonterminals.Count == 0)
                        netMetric[prodIndex] = prodMetric[prodIndex][rightIndex] = 1;
                    else
                        prodMetric[prodIndex][rightIndex] = -1;//not aclculated -1
            }
            bool found = true;
            while (found)//stop when exceed needed accuracy
            {
                found = false;
                for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
                    for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                    {
                        var right = productions[prodIndex].rights[rightIndex];
                        if (right.nonterminals.Count != 0)
                        {//if production contains neterminal - calculating  
                            double rightSum = countWordMetric(right, rs, netMetric, productions);
                            if (Math.Abs(prodMetric[prodIndex][rightIndex] - rightSum) > prodMetric[prodIndex][rightIndex] * 0.05)//eps 
                            {
                                found = true;
                                prodMetric[prodIndex][rightIndex] = rightSum;
                                netMetric[prodIndex] = Math.Max(netMetric[prodIndex], rightSum);
                            }
                        }
                    }
            }
        }

        public static double countWordMetric(SimplifiedWord word,
            RandomSettings rs,
            double[] netMetric,
            List<SimplifiedProductionGroup> productions)
        {
            int productionsCount = productions.Count;
            double rightSum = 1;
            foreach (var neterminal in word.nonterminals)
            {
                double sum = 0;
                for (int i = 0; i < productionsCount; ++i)
                    if (netMetric[i] != -1 && productions[i].Left == neterminal.Key)
                        sum += netMetric[i] * rs.getProductionPossibility(i) / rs.getTotalPossibility();
                sum = Math.Pow(sum, neterminal.Value);
                rightSum *= sum;
            }
            return rightSum;
        }

        public static void countBetterMetric(List<SimplifiedProductionGroup> productions,
            RandomSettings rs,
            out double[] netMetric,
            out double[][] prodMetric
            )
        {
            netMetric = new double[productions.Count];
            prodMetric = new double[productions.Count][];
            int productionsCount = productions.Count;
            const double eps = 1e-9;

            //initialization
            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
            {
                netMetric[prodIndex] = -1;
                prodMetric[prodIndex] = new double[productions[prodIndex].RightSize];
                for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                    if (productions[prodIndex].rights[rightIndex].nonterminals.Count == 0)
                        netMetric[prodIndex] = prodMetric[prodIndex][rightIndex] = 1;
                    else
                        prodMetric[prodIndex][rightIndex] = -1;//not aclculated -1
            }
            bool found = true;
            while (found)//stop when exceed needed accuracy
            {
                found = false;
                for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
                    for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                    {
                        var right = productions[prodIndex].rights[rightIndex];
                        if (right.nonterminals.Count != 0)
                        {//if production contains neterminal - calculating  
                            double rightSum = countBetterWordMetric(right, rs, netMetric, productions);
                            if (Math.Abs(prodMetric[prodIndex][rightIndex] - rightSum) > prodMetric[prodIndex][rightIndex] * 0.05)//eps 
                            {
                                found = true;
                                prodMetric[prodIndex][rightIndex] = rightSum;
                                netMetric[prodIndex] = Math.Max(netMetric[prodIndex], rightSum);
                            }
                        }
                    }
            }
        }

        public static double countBetterWordMetric(SimplifiedWord word,
            RandomSettings rs,
            double[] netMetric,
            List<SimplifiedProductionGroup> productions)
        {
            int productionsCount = productions.Count;
            double min = 1;
            int count = 0;
            foreach (var neterminal in word.nonterminals)
            {
                double sum = 0;
                for (int i = 0; i < productionsCount; ++i)
                    if (netMetric[i] != -1 && productions[i].Left == neterminal.Key)
                        sum += netMetric[i] * rs.getProductionPossibility(i) / rs.getTotalPossibility();
                min = Math.Min(min, sum);
                count += neterminal.Value;
                //sum = sum / neterminal.Value;
                //rightSum = Math.Min(sum,rightSum);
            }
            if(count>0)
                return min/count;
            else 
                return 1;
        }

        public static void countStupidMetric(List<SimplifiedProductionGroup> productions,
            out int[] netMetric,
            out int[][] prodMetric
            )
        {
            netMetric = new int[productions.Count];
            prodMetric = new int[productions.Count][];
            int productionsCount = productions.Count;

            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
                prodMetric[prodIndex] = new int[productions[prodIndex].RightSize];

            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
            {
                for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                {
                    var right = productions[prodIndex].rights[rightIndex];
                    int rightSum = countWordStupidMetric(right);
                    prodMetric[prodIndex][rightIndex] = rightSum;
                    netMetric[prodIndex] = Math.Min(netMetric[prodIndex], rightSum);
                }
            }
        }

        public static int countWordStupidMetric(SimplifiedWord word)
        {
            int rightSum = 0;
            foreach (var neterminal in word.nonterminals)
            {
                rightSum += neterminal.Value;
            }
            return rightSum;
        }

        public static void applyMove(PrimaryMove move, List<string> words, List<ProductionGroup> prods)
        {
            ProductionGroup prod = prods[move.ProductionGroupNumber];
            if (move.WordNumber != -1)
            {
                string word = words[move.WordNumber];
                int letterIndex = StrategyUtilitiesClass.isHaveLetter(word, prod.Left);
                string newWord = word.Substring(0, letterIndex) +
                         prod.getRightAt(move.ProductionNumber) +
                         word.Substring(letterIndex + 1, word.Length - letterIndex - 1);
                words[move.WordNumber] = newWord;
            }
            else
            {
                string newWord = prod.getRightAt(move.ProductionNumber);
                words.Add(newWord);
            }
        }

        public static void applyMove(Move move, Bank bank, SimplifiedWord word, List<SimplifiedProductionGroup> prods)
        {
            foreach (var m in move.getMoves())
            {
                bank.removeProduction(m.ProductionGroupNumber);
                var prod = prods[m.ProductionGroupNumber];
                word.addNonterminal(prod.Left, -1);
                word.terminals += prod.rights[m.ProductionNumber].terminals;
                foreach (var neterminal in prod.rights[m.ProductionNumber].nonterminals)
                    word.addNonterminal(neterminal.Key, -neterminal.Value);
            }
        }

        public static void applyMove(PrimaryMove move, List<SimplifiedWord> simpleWords, List<SimplifiedProductionGroup> prods)
        {
            var prod = prods[move.ProductionGroupNumber];
            if (move.WordNumber != -1)
            {
                SimplifiedWord word = simpleWords[move.WordNumber];
                word.addNonterminal(prod.Left, -1);
                word.terminals += prod.rights[move.ProductionNumber].terminals;
                foreach (var neterminal in prod.rights[move.ProductionNumber].nonterminals)
                    word.addNonterminal(neterminal.Key, neterminal.Value);
            }
            else
            {
                SimplifiedWord word = new SimplifiedWord(prod.rights[move.ProductionNumber]);
                simpleWords.Add(word);
            }
        }
    }
}
