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
        double[] netMetric;
        double[][] prodsMetric;
        int[] bestProd;

        public ShortWordsStrategy():base("Short Words Strategy") { 
            this.GameSettingsChanged += beforeStart;
        }

        protected void beforeStart(object sender, EventArgs e)
        {
            StrategyUtilitiesClass.countMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                double maxMetric = -1;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (maxMetric == -1 || prodsMetric[i][j] > maxMetric ||
                        prodsMetric[i][j] == maxMetric &&
                        simplifiedProductions[i].rights[j].terminals > simplifiedProductions[i].rights[bestProd[j]].terminals)
                    {
                        maxMetric = prodsMetric[i][j];
                        bestProd[i] = j;
                    }
                }
            }
        }

        public override Move makeMove(int playerNumber,
            int MoveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank)
        {
            Move move = new Move();

            //get simplified form of words
            List<SimplifiedWord> simpleWords = simplifiedWords[playerNumber];

            while (true)
            {
                PrimaryMove primaryMove;
                if (move.MovesCount == 0)
                    primaryMove = findFirstMove(simpleWords, productionNumber);
                else
                    primaryMove = findMove( simpleWords,  bank);
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
            int prosuctionGroupNumber;
            List<List<int>> allowedWords = new List<List<int>>();
            foreach (var pr in simplifiedProductions)//find words allowed for productions
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                if (pr.Left == 'S')//if can create new word
                    allowedWords.Last().Add(-1);
            }

            //select production from bank
            {
                prosuctionGroupNumber = -1;
                double maxMetric = -1;
                for (int i = 0; i < simplifiedProductions.Count; ++i)
                    if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                        if (netMetric[i] > maxMetric)
                        {
                            maxMetric = netMetric[i];
                            prosuctionGroupNumber = i;
                        }
                if (prosuctionGroupNumber == -1)
                    return null;//not found production
            }

            SimplifiedProductionGroup prod = simplifiedProductions[prosuctionGroupNumber];
            int productionNumber = bestProd[prosuctionGroupNumber];
            int wordNumber = -1;
            //select worfd with better metric
            {
                double maxMetric = -1;
                for (int i = 0; i < allowedWords[prosuctionGroupNumber].Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[prosuctionGroupNumber][i] == -1)
                        word = new SimplifiedWord("" + simplifiedProductions[prosuctionGroupNumber].Left);
                    else
                        word = simpleWords[allowedWords[prosuctionGroupNumber][i]];
                    double metric = StrategyUtilitiesClass.countWordMetric(word,
                        rs,
                        netMetric,
                        simplifiedProductions);
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

        PrimaryMove findFirstMove(List<SimplifiedWord> simpleWords, int productionGroupNumber)
        {
            int groupNumber;
            var prod = simplifiedProductions[productionGroupNumber];
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
                        word = new SimplifiedWord("" + simplifiedProductions[groupNumber].Left);
                    else
                        word = simpleWords[allowedWords[i]];
                    double metric = StrategyUtilitiesClass.countWordMetric(word,
                        rs,
                        netMetric,
                        simplifiedProductions);
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
