﻿using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategies
{
    public class StupidShortWordsStrategy : Strategy
    {
        int[] netMetric;
        int[][] prodsMetric;
        int[] bestProd;

        public StupidShortWordsStrategy() : base() {
            Name = "Stupid Short Words Strategy";
            ShortName ="SSWS";
            this.GameSettingsChanged += beforeStart;
        }

        protected void beforeStart(object sender,EventArgs e)
        {
            StrategyUtilitiesClass.countStupidMetric(simplifiedProductions, out netMetric, out prodsMetric);

            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                int minMetric = int.MaxValue;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (prodsMetric[i][j] < minMetric ||
                        prodsMetric[i][j] == minMetric &&
                        simplifiedProductions[i][j].terminals > simplifiedProductions[i][bestProd[j]].terminals)
                    {
                        minMetric = prodsMetric[i][j];
                        bestProd[i] = j;
                    }
                }
            }
        }

        public override Move makeMove(int playerNumber,
            int moveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank)
        {
            Move move = new Move();

            List<SimplifiedWord> simpleWords = simplifiedWords[playerNumber];
            //get simplified form of words

            while (true)
            {
                PrimaryMove primaryMove;
                if (move.MovesCount == 0)//make first move
                    primaryMove = findFirstMove(simpleWords, productionNumber);
                else//make move from bank
                    primaryMove = findMove(simpleWords, bank);

                if (primaryMove != null)
                {
                    if (move.MovesCount != 0)//delete production from bank
                        bank.removeProduction(primaryMove.ProductionGroupNumber);
                    //make this move
                    move.addMove(primaryMove);
                    StrategyUtilitiesClass.applyMove(primaryMove, simpleWords, simplifiedProductions);
                }
                else
                    break;
            }
            return move;
        }

        PrimaryMove findMove(List<SimplifiedWord> simpleWords, Bank bank)
        {
            int groupNumber;
            List<List<int>> allowedWords = new List<List<int>>();
            foreach (var pr in simplifiedProductions)//find words allowed for productions
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));

            //select production from bank
            {//find production with better metric
                groupNumber = -1;
                int minMetric = int.MaxValue;
                for (int i = 0; i < simplifiedProductions.Count; ++i)
                    if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                        if (netMetric[i] < minMetric)
                        {
                            minMetric = netMetric[i];
                            groupNumber = i;
                        }
                if (groupNumber == -1)
                    return null;//not found production
            }

            SimplifiedProductionGroup prod = simplifiedProductions[groupNumber];
            int productionNumber = bestProd[groupNumber];
            int wordNumber = -1;
            //select worfd with better metric
            {
                int minMetric = int.MaxValue;
                for (int i = 0; i < allowedWords[groupNumber].Count; ++i)
                {
                    SimplifiedWord word = simpleWords[allowedWords[groupNumber][i]];
                    int metric = StrategyUtilitiesClass.countWordStupidMetric(word);
                    if (metric < minMetric)
                    {
                        minMetric = metric;
                        wordNumber = i;
                    }
                }
                wordNumber = allowedWords[groupNumber][wordNumber];
            }
            return new PrimaryMove(wordNumber, groupNumber, productionNumber);
        }

        PrimaryMove findFirstMove(List<SimplifiedWord> simpleWords,int productionGroupNumber)
        {
            int groupNumber;
            var prod = simplifiedProductions[productionGroupNumber];
            List<int> allowedWords = new List<int>();
            allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left);
            if (simplifier.GetChar(prod.Left) == 'S')//if can create new word
                allowedWords.Add(-1);

            if (allowedWords.Count == 0)
                return null;
            groupNumber = productionGroupNumber;

            int productionNumber = bestProd[groupNumber];
            int wordNumber = -1;
            //select word with better metric
            {
                int minMetric = int.MaxValue;
                for (int i = 0; i < allowedWords.Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[i] == -1)
                        word = simplifier.ConvertWord("S");
                    else
                        word = simpleWords[allowedWords[i]];
                    int metric = StrategyUtilitiesClass.countWordStupidMetric(word);
                    if (metric < minMetric)
                    {
                        minMetric = metric;
                        wordNumber = i;
                    }
                }
                wordNumber = allowedWords[wordNumber];
            }
            return new PrimaryMove(wordNumber, groupNumber, productionNumber);
        }
    }
}
