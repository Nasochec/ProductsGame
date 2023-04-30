using ProductionsGameCore;
using ProductsGame;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace OneMoveStrategy
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

            //gameSettings = GameSettings.ReadFromFile(@"./conf2.xml");
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

            List<SimplifiedWord> myWords = new List<SimplifiedWord>();

            //Преобразууем продукции к упрощенному виду, чтобы алгоритм работал быстрее.
            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();

            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            //Высчитываем метрику оценки продукций. При выборе продукции будем брать ту, у которой значение метрикик больше.
            double[] netMetric;
            double[][] prodsMetric;
            findSecondMetric(prods, gameSettings, out netMetric, out prodsMetric);



            for (int moveIndex = 0; moveIndex < gameSettings.NumberOfMoves; moveIndex++)
            {
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);

                Move move = new Move();

                //преобразуем слова к упрощенному виду, чтобы алогритм работал быстрее.
                myWords.Clear();
                foreach (var word in words[playerNumber])
                {
                    myWords.Add(new SimplifiedWord(word));
                }

                int groupNumber;

                while (true)
                {







                    //if (wordnumber != -1)
                    //{
                    //    string word = words[playerNumber][wordnumber];
                    //    int letterIndex = StrategyUtilities.isHaveLetter(word, prod.Left);
                    //    string newWord = word.Substring(0, letterIndex) +
                    //             prod.getRightAt(productionNumber) +
                    //             word.Substring(letterIndex + 1, word.Length - letterIndex - 1);
                    //    words[playerNumber][wordnumber] = newWord;
                    //}
                    //else
                    //{
                    //    string newWord = prod.getRightAt(productionNumber);
                    //    words[playerNumber].Add(newWord);
                    //}
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
        public static void findSecondMetric(List<SimplifiedProductionGroup> productions,
            GameSettings settings,
            out double[] netMetric,
            out double[][] prodMetric
            )
        {
            netMetric = new double[productions.Count];
            prodMetric = new double[productions.Count][];
            int productionsCount = productions.Count;
            const double eps = 0.00001;


            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
            {
                netMetric[prodIndex] = -1;
                prodMetric[prodIndex] = new double[productions[prodIndex].RightSize];
                for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                {
                    var right = productions[prodIndex].rights[rightIndex];
                    if (right.neterminalsCount.Count == 0)
                        netMetric[prodIndex] = prodMetric[prodIndex][rightIndex] = 1;
                    else
                        prodMetric[prodIndex][rightIndex] = -1;
                }
            }
            RandomSettings rs = settings.RandomSettings;
            bool found = true;
            while (found)
            {
                found = false;
                for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
                {
                    for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                    {
                        var right = productions[prodIndex].rights[rightIndex];
                        if (right.neterminalsCount.Count != 0)
                        {//если в продукции есть нетерминалы - высчитываем её
                            double rightSum = 1;
                            foreach (var neterminal in right.neterminalsCount)
                            {
                                double sum = 0;
                                for (int i = 0; i < productionsCount; ++i)
                                {
                                    if (netMetric[i] != -1 && productions[i].Left == neterminal.Key)
                                    {
                                        sum += netMetric[i] * rs.getProductionPossibility(i) / rs.getTotalPossibility();
                                    }
                                }
                                sum = Math.Pow(sum, neterminal.Value);
                                rightSum *= sum;
                            }
                            if (Math.Abs( prodMetric[prodIndex][rightIndex] - rightSum) >= eps)
                            {
                                found = true;
                                prodMetric[prodIndex][rightIndex] = rightSum;
                                netMetric[prodIndex] = Math.Max(netMetric[prodIndex], rightSum);
                            }
                        }
                    }
                }
            }
        }
    }
}
