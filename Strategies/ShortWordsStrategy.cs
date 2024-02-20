using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategies
{
    public class ShortWordsStrategy : Strategy
    {
        List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();
        List<SimplifiedProductionGroup> simplifiedProds = new List<SimplifiedProductionGroup>();
        double[] netMetric;
        double[][] prodsMetric;
        int[] bestProd;

        public ShortWordsStrategy():base("Short Words Strategy") { }

        protected override void beforeStart()
        {
            //get the simplified form of productions
           
            for (int i = 0; i < Settings.ProductionsCount; ++i)
                simplifiedProds.Add(new SimplifiedProductionGroup(Settings.getProductionGroup(i)));
            StrategyUtilitiesClass.countMetric(simplifiedProds, Settings, out netMetric, out prodsMetric);
            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                double maxMetric = -1;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (maxMetric == -1 || prodsMetric[i][j] > maxMetric ||
                        prodsMetric[i][j] == maxMetric &&
                        simplifiedProds[i].rights[j].terminals > simplifiedProds[i].rights[bestProd[j]].terminals)
                    {
                        maxMetric = prodsMetric[i][j];
                        bestProd[i] = j;
                    }
                }
            }
        }

        public override Move makeMove(int productionNumber, int MoveNumber, List<List<string>> words, Bank bank)
        {
            Move move = new Move();

            //get simplified form of words
            simpleWords.Clear();
            foreach (var word in words[PlayerNumber])
                simpleWords.Add(new SimplifiedWord(word));

            while (true)
            {
                PrimaryMove primaryMove;
                if (move.MovesCount == 0)
                    primaryMove = findFirstMove(simplifiedProds, simpleWords, Settings.RandomSettings, bank, netMetric, bestProd, productionNumber);
                else
                    primaryMove = findMove(simplifiedProds, simpleWords, Settings.RandomSettings, bank, netMetric, bestProd);
                if (primaryMove != null)
                {
                    if (move.MovesCount != 0)//delete production from bank
                        bank.removeProduction(primaryMove.ProductionGroupNumber);
                    //make this move
                    move.addMove(primaryMove);
                    StrategyUtilitiesClass.applyMove(primaryMove, simpleWords, simplifiedProds);
                }
                else
                    break;
            }
            return move;
        }

        PrimaryMove findMove(List<SimplifiedProductionGroup> prods, List<SimplifiedWord> simpleWords, RandomSettings rs, Bank bank, double[] netMetric, int[] bestProd)
        {
            int prosuctionGroupNumber;
            List<List<int>> allowedWords = new List<List<int>>();
            foreach (var pr in prods)//find words allowed for productions
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                if (pr.Left == 'S')//if can create new word
                    allowedWords.Last().Add(-1);
            }

            //select production from bank
            {
                prosuctionGroupNumber = -1;
                double maxMetric = -1;
                for (int i = 0; i < prods.Count; ++i)
                    if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                        if (netMetric[i] > maxMetric)
                        {
                            maxMetric = netMetric[i];
                            prosuctionGroupNumber = i;
                        }
                if (prosuctionGroupNumber == -1)
                    return null;//not found production
            }

            SimplifiedProductionGroup prod = prods[prosuctionGroupNumber];
            int productionNumber = bestProd[prosuctionGroupNumber];
            int wordNumber = -1;
            //select worfd with better metric
            {
                double maxMetric = -1;
                for (int i = 0; i < allowedWords[prosuctionGroupNumber].Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[prosuctionGroupNumber][i] == -1)
                        word = new SimplifiedWord("" + prods[prosuctionGroupNumber].Left);
                    else
                        word = simpleWords[allowedWords[prosuctionGroupNumber][i]];
                    double metric = StrategyUtilitiesClass.countWordMetric(word,
                        rs,
                        netMetric,
                        prods);
                    if (metric > maxMetric)
                    {
                        maxMetric = metric;
                        wordNumber = i;
                    }
                }
                wordNumber = allowedWords[prosuctionGroupNumber][wordNumber];
            }
            return new PrimaryMove(wordNumber, prosuctionGroupNumber, productionNumber);
        }

        PrimaryMove findFirstMove(List<SimplifiedProductionGroup> prods, List<SimplifiedWord> simpleWords, RandomSettings rs, Bank bank, double[] netMetric, int[] bestProd, int productionGroupNumber)
        {
            int groupNumber;
            var prod = prods[productionGroupNumber];
            List<int> allowedWords = new List<int>();

            allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left);
            if (prod.Left == 'S')//if can create new word
                allowedWords.Add(-1);

            if (allowedWords.Count == 0)
                return null;
            groupNumber = productionGroupNumber;

            int productionNumber = bestProd[groupNumber];
            int wordNumber = -1;
            //select word with better metric
            {
                double maxMetric = -1;
                for (int i = 0; i < allowedWords.Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[i] == -1)
                        word = new SimplifiedWord("" + prods[groupNumber].Left);
                    else
                        word = simpleWords[allowedWords[i]];
                    double metric = StrategyUtilitiesClass.countWordMetric(word,
                        rs,
                        netMetric,
                        prods);
                    if (metric > maxMetric)
                    {
                        maxMetric = metric;
                        wordNumber = i;
                    }
                }
                wordNumber = allowedWords[wordNumber];
            }
            return new PrimaryMove(wordNumber, groupNumber, productionNumber);
        }
    }
}
