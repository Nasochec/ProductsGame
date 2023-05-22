using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StupidShortWordsStrategy
{
    internal class StupidShortWordsStrategy
    {
        static void Main(string[] args)
        {
            Bank bank;
            GameSettings gameSettings;
            int playerNumber;
            List<List<string>> words;
            int productionGroupNumber;
            int moveNumber;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream inputStream = Console.OpenStandardInput();

            //reading constant values
            gameSettings = (GameSettings)formatter.Deserialize(inputStream);
            playerNumber = (int)formatter.Deserialize(inputStream);

            List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

            //get the simplified form of productions
            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();
            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            //count metric of broductions
            int[] netMetric;
            int[][] prodsMetric;
            StrategyUtilitiesClass.countStupidMetric(prods, out netMetric, out prodsMetric);

            //find best production in group
            int[] bestProd;
            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                int minMetric = int.MaxValue;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (prodsMetric[i][j] < minMetric ||
                        prodsMetric[i][j] == minMetric &&
                        prods[i].rights[j].terminals > prods[i].rights[bestProd[j]].terminals)
                    {
                        minMetric = prodsMetric[i][j];
                        bestProd[i] = j;
                    }
                }
            }

            for (int moveIndex = 0; moveIndex < gameSettings.NumberOfMoves; moveIndex++)
            {
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);

                Move move = new Move();

                //get simplified form of words
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                    simpleWords.Add(new SimplifiedWord(word));

                int groupNumber;

                while (true)
                {
                    PrimaryMove primaryMove;
                    if (move.MovesCount == 0)//make first move
                        primaryMove = findFirstMove(prods, simpleWords, bank, netMetric, bestProd, productionGroupNumber);
                    else//make move from bank
                        primaryMove = findMove(prods, simpleWords, bank, netMetric, bestProd);

                    if (primaryMove != null)
                    {
                        if (move.MovesCount != 0)//delete production from bank
                            bank.removeProduction(primaryMove.ProductionGroupNumber);
                        //make this move
                        move.addMove(primaryMove);
                        applyMove(primaryMove, simpleWords, prods);
                    }
                    else
                        break;
                }
                Console.Out.WriteLine(move);
            }
            return;
        }


        static PrimaryMove findMove(List<SimplifiedProductionGroup> prods, List<SimplifiedWord> simpleWords, Bank bank, int[] netMetric, int[] bestProd)
        {
            int groupNumber;
            List<List<int>> allowedWords = new List<List<int>>();
            foreach (var pr in prods)//find words allowed for productions
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                if (pr.Left == 'S')//if can create new word
                    allowedWords.Last().Add(-1);
            }

            //select production from bank
            {//find production with better metric
                groupNumber = -1;
                int minMetric = int.MaxValue;
                for (int i = 0; i < prods.Count; ++i)
                    if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                        if (netMetric[i] < minMetric)
                        {
                            minMetric = netMetric[i];
                            groupNumber = i;
                        }
                if (groupNumber == -1)
                    return null;//not found production
            }

            SimplifiedProductionGroup prod = prods[groupNumber];
            int productionNumber = bestProd[groupNumber];
            int wordNumber = -1;
            //select worfd with better metric
            {
                int minMetric = int.MaxValue;
                for (int i = 0; i < allowedWords[groupNumber].Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[groupNumber][i] == -1)
                        word = new SimplifiedWord("" + prods[groupNumber].Left);
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

        static PrimaryMove findFirstMove(List<SimplifiedProductionGroup> prods, List<SimplifiedWord> simpleWords, Bank bank, int[] netMetric, int[] bestProd, int productionGroupNumber)
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
                int minMetric = int.MaxValue;
                for (int i = 0; i < allowedWords.Count; ++i)
                {
                    SimplifiedWord word;
                    if (allowedWords[i] == -1)
                        word = new SimplifiedWord("" + prods[groupNumber].Left);
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

        static void applyMove(PrimaryMove move,List<SimplifiedWord> simpleWords,List<SimplifiedProductionGroup> prods) {
            var prod = prods[move.ProductionGroupNumber];
            if (move.WordNumber!= -1)
            {
                SimplifiedWord word = simpleWords[move.WordNumber];
                word.addNeterminal(prod.Left, -1);
                word.terminals += prod.rights[move.ProductionNumber].terminals;
                foreach (var neterminal in prod.rights[move.ProductionNumber].neterminalsCount)
                    word.addNeterminal(neterminal.Key, neterminal.Value);
            }
            else
            {
                SimplifiedWord word = new SimplifiedWord(prod.rights[move.ProductionNumber]);
                simpleWords.Add(word);
            }
        }
    }
}
