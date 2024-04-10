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
    public class StupidShortWordsStrategy : Strategy
    {
        int[] netMetric;
        int[][] prodsMetric;
        int[] bestProd;
        List<SimplifiedProductionGroup> simplifiedProds = new List<SimplifiedProductionGroup>();
        List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

        public StupidShortWordsStrategy() : base("Stupid Short Words Strategy") { }

        protected override void beforeStart()
        {
            for (int i = 0; i < Settings.ProductionsCount; ++i)
                simplifiedProds.Add(new SimplifiedProductionGroup(Settings.getProductionGroup(i)));
            StrategyUtilitiesClass.countStupidMetric(simplifiedProds, out netMetric, out prodsMetric);


            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                int minMetric = int.MaxValue;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (prodsMetric[i][j] < minMetric ||
                        prodsMetric[i][j] == minMetric &&
                        simplifiedProds[i].rights[j].terminals > simplifiedProds[i].rights[bestProd[j]].terminals)
                    {
                        minMetric = prodsMetric[i][j];
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
                if (move.MovesCount == 0)//make first move
                    primaryMove = findFirstMove(simpleWords, bank, productionNumber);
                else//make move from bank
                    primaryMove = findMove(simpleWords, bank);

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

        PrimaryMove findMove(List<SimplifiedWord> simpleWords, Bank bank)
        {
            int groupNumber;
            List<List<int>> allowedWords = new List<List<int>>();
            foreach (var pr in simplifiedProds)//find words allowed for productions
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                if (pr.Left == 'S')//if can create new word
                    allowedWords.Last().Add(-1);
            }

            //select production from bank
            {//find production with better metric
                groupNumber = -1;
                int minMetric = int.MaxValue;
                for (int i = 0; i < simplifiedProds.Count; ++i)
                    if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                        if (netMetric[i] < minMetric)
                        {
                            minMetric = netMetric[i];
                            groupNumber = i;
                        }
                if (groupNumber == -1)
                    return null;//not found production
            }

            SimplifiedProductionGroup prod = simplifiedProds[groupNumber];
            int productionNumber = bestProd[groupNumber];
            int wordNumber = -1;
            //select worfd with better metric
            {
                int minMetric = int.MaxValue;
                for (int i = 0; i < allowedWords[groupNumber].Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[groupNumber][i] == -1)
                        word = new SimplifiedWord("" + simplifiedProds[groupNumber].Left);
                    else
                        word = simpleWords[allowedWords[groupNumber][i]];
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

        PrimaryMove findFirstMove(List<SimplifiedWord> simpleWords, Bank bank,int productionGroupNumber)
        {
            int groupNumber;
            var prod = simplifiedProds[productionGroupNumber];
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
                int minMetric = int.MaxValue;
                for (int i = 0; i < allowedWords.Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[i] == -1)
                        word = new SimplifiedWord("" + simplifiedProds[groupNumber].Left);
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
