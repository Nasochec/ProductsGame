﻿using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategies
{
    public class SmartRandomStrategy:RandomStrategy
    {
        double[] netMetric;
        double[][] prodsMetric;
        //int[] bestProd;

        public SmartRandomStrategy():base() {
            Name = "Smart Random Strategy";
            this.GameSettingsChanged += beforeStart;
        }

        protected void beforeStart(object sender,EventArgs e) {
            //get the simplified form of productions

            for (int i = 0; i < GameSettings.ProductionsCount; ++i)
                simplifiedProductions.Add(new SimplifiedProductionGroup(GameSettings.getProductionGroup(i)));
            StrategyUtilitiesClass.countMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
        }

        private int getRand(IEnumerable<double> weights) {
            double sum = 0;
            foreach (double weight in weights)
                sum+= Math.Sqrt(weight);
            double rand = random.NextDouble() * sum;
            int i = 0;
            foreach (double weight in weights)
            {
                rand -= Math.Sqrt(weight);
                if (rand <= 0)
                    return i;
                i++;
            }
            return 0;
        }


        protected override PrimaryMove findFirstMove(List<string> words, int productionGroupNumber)
        {
            List<int> allowedWords;

            var prod = productions[productionGroupNumber];
            allowedWords = StrategyUtilitiesClass.findMatches(words, prod.Left);
            if (prod.Left == 'S')//if can create new word
                allowedWords.Add(-1);

            if (allowedWords.Count == 0)
                return null;//not found production

            int productionNumber = getRand(prodsMetric[productionGroupNumber]);
            List<double> wordsMetrics = new List<double>();
            foreach (int wordIndex in allowedWords)
            {
                var word = (wordIndex == -1) ? "S" : words[wordIndex];
                wordsMetrics.Add(StrategyUtilitiesClass.countWordMetric(new SimplifiedWord(word), rs, netMetric, simplifiedProductions));
            }
            int wordnumber = getRand(wordsMetrics);//select word
                //random.Next(allowedWords.Count);
            wordnumber = allowedWords[wordnumber];

            return new PrimaryMove(wordnumber, productionGroupNumber, productionNumber);
        }

        protected override PrimaryMove findMove(List<string> words, Bank bank)
        {
            List<List<int>> allowedWords = new List<List<int>>();
            int productionGroupNumber;
            foreach (var pr in productions)//find words allowed for productions
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(words, pr.Left));
                if (pr.Left == 'S')//if can create new word
                    allowedWords.Last().Add(-1);
            }

            List<int> allowedGroupIndexes = new List<int>();
            List<double> allowedGroupMetrics = new List<double>();
            for (int i = 0; i < productions.Count; ++i)
                if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                {
                    allowedGroupIndexes.Add(i);
                    allowedGroupMetrics.Add(netMetric[i]);
                }
            if (allowedGroupIndexes.Count == 0)
                return null;//not found production

            productionGroupNumber = allowedGroupIndexes[getRand(allowedGroupMetrics)];
            int productionNumber = getRand(prodsMetric[productionGroupNumber]);

            List<double> wordsMetrics = new List<double>();
            foreach (int wordIndex in allowedWords[productionGroupNumber])
            {
                wordsMetrics.Add(StrategyUtilitiesClass.countWordMetric(new SimplifiedWord(words[wordIndex]), rs, netMetric, simplifiedProductions));
            }
            int wordnumber = getRand(wordsMetrics);//select word

            wordnumber = allowedWords[productionGroupNumber][wordnumber];
            return new PrimaryMove(wordnumber, productionGroupNumber, productionNumber);
        }
    }
}
