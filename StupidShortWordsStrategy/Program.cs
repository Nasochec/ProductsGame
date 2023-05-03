using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StupidShortWordsStrategy
{
    internal class Program
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

            gameSettings = (GameSettings)formatter.Deserialize(inputStream);
            playerNumber = (int)formatter.Deserialize(inputStream);

            //gameSettings = GameSettings.ReadFromFile(@"./conf1.xml");
            //playerNumber = 0;
            //bank = new Bank(gameSettings.ProductionsCount);
            //for (int i = 0; i < gameSettings.ProductionsCount; i++)
            //{
            //    bank.addProduction(i);
            //    //bank.addProduction(i);
            //}
            //words = new List<List<string>>();
            //words.Add(new List<string>());
            //words.Add(new List<string>());
            //productionGroupNumber = 0;

            List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

            //Преобразууем продукции к упрощенному виду, чтобы алгоритм работал быстрее.
            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();

            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            //Высчитываем метрику оценки продукций. При выборе продукции будем брать ту, у которой значение метрикик больше.
            int[] netMetric;
            int[][] prodsMetric;
            countStupidMetric(prods, out netMetric, out prodsMetric);

            //Найдём для каждой группы продукций лучшую продукцию.
            int[] bestProd;
            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                int minMetric = int.MaxValue;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (prodsMetric[i][j] < minMetric)
                    {
                        minMetric = prodsMetric[i][j];
                        bestProd[i] = j;
                    }
                    else if (prodsMetric[i][j] == minMetric &&
                        prods[i].getRightTerminalsAt(j) > prods[i].getRightTerminalsAt(bestProd[i]))
                    {
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

                //преобразуем слова к упрощенному виду, чтобы алогритм работал быстрее.
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                {
                    simpleWords.Add(new SimplifiedWord(word));
                }

                int groupNumber;

                while (true)
                {
                    List<List<int>> allowedWords = new List<List<int>>();
                    foreach (var pr in prods)//находим слова допустимые для каждой продукции
                    {
                        allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                        if (pr.Left == 'S')//если можно создать новое слово
                            allowedWords.Last().Add(-1);
                    }

                    if (move.MovesCount > 0)//выбираем номер группы продукций
                    {//выбираем продукцию из банка
                        groupNumber = -1;
                        int minMetric = int.MaxValue;
                        for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                            if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                                if (netMetric[i] < minMetric)
                                {
                                    minMetric = netMetric[i];
                                    groupNumber = i;
                                }
                        if (groupNumber == -1)
                            break;//не нашлось продукций доступных к применению
                    }
                    else//если это первый ход - обязаны применить выпавшую продукцию
                    {
                        if (allowedWords[productionGroupNumber].Count == 0)
                            break;
                        groupNumber = productionGroupNumber;
                    }

                    SimplifiedProductionGroup prod = prods[groupNumber];
                    int productionNumber = bestProd[groupNumber];//выбираем номер продукции в группе
                    int wordNumber = -1;
                    //выбираем слово. Выберем то слово, у кторого самая большая метрика
                    {
                        int minMetric = int.MaxValue;
                        for (int i = 0; i < allowedWords[groupNumber].Count; ++i)
                        {
                            SimplifiedWord word;
                            if (allowedWords[groupNumber][i] == -1)
                                word = new SimplifiedWord("" + prods[groupNumber].Left);
                            else
                                word = simpleWords[allowedWords[groupNumber][i]];
                            int metric = countStupidMetric(word);
                            if (metric < minMetric)
                            {
                                minMetric = metric;
                                wordNumber = i;
                            }

                        }
                        wordNumber = allowedWords[groupNumber][wordNumber];
                    }


                    if (move.MovesCount >= 1)//удаляем продукцию из банка если надо
                        bank.removeProduction(groupNumber);
                    //теперь совершим этот ход
                    move.addMove(wordNumber, groupNumber, productionNumber);


                    if (wordNumber != -1)
                    {
                        SimplifiedWord word = simpleWords[wordNumber];
                        word.neterminalsCount[prod.Left]--;
                        word.terminals += prod.rights[productionNumber].terminals;
                        foreach (var neterminal in prod.rights[productionNumber].neterminalsCount)
                            word.addNeterminal(neterminal.Key, neterminal.Value);
                    }
                    else
                    {
                        SimplifiedWord word = new SimplifiedWord(prods[groupNumber].rights[productionNumber]);
                        simpleWords.Add(word);
                    }
                }
                Console.Out.WriteLine(move);
            }
            return;
        }


        /// <summary>
        /// Находит метрику оценки продукций. 
        /// Предполагается что чем больше её значение тем за меньшее количество ходов можно получить терминальную строку.
        /// </summary>
        /// <param name="productions"></param>
        /// <param name="settings"></param>
        /// <param name="netMetric"></param>
        /// <param name="prodMetric"></param>
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

                    int rightSum = countStupidMetric(right);
                    prodMetric[prodIndex][rightIndex] = rightSum;
                    netMetric[prodIndex] = Math.Min(netMetric[prodIndex], rightSum);
                }
            }

        }

        public static int countStupidMetric(SimplifiedWord word)
        {
            int rightSum = 0;
            foreach (var neterminal in word.neterminalsCount)
            {
                rightSum += neterminal.Value;
            }
            return rightSum;
        }


    }

}
