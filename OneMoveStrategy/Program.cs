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

            //gameSettings = (GameSettings)formatter.Deserialize(inputStream);
            //playerNumber = (int)formatter.Deserialize(inputStream);

            gameSettings = GameSettings.ReadFromFile(@"./conf2.xml");
            playerNumber = 0;
            bank = new Bank(gameSettings.ProductionsCount);
            for (int i = 0; i < gameSettings.ProductionsCount; i++)
            {
                bank.addProduction(i);
                bank.addProduction(i);
                bank.addProduction(i);
                bank.addProduction(i);
                bank.addProduction(i);
                bank.addProduction(i);
                bank.addProduction(i);
            }
            words = new List<List<string>>();
            words.Add(new List<string>());
            words.Add(new List<string>());
            productionGroupNumber = 0;

            List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

            //Преобразууем продукции к упрощенному виду, чтобы алгоритм работал быстрее.
            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();

            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            //Высчитываем метрику оценки продукций. При выборе продукции будем брать ту, у которой значение метрикик больше.
            //double[] netMetric;
            //double[][] prodsMetric;
            //countMetric(prods, gameSettings, out netMetric, out prodsMetric);

            ////Найдём для каждой группы продукций лучшую продукцию.
            //int[] bestProd;
            //bestProd = new int[netMetric.Length];
            //for (int i = 0; i < bestProd.Length; ++i)
            //{
            //    double maxMetric = -1;
            //    for (int j = 0; j < prodsMetric[i].Length; ++j)
            //    {
            //        if (prodsMetric[i][j] > maxMetric)
            //        {
            //            maxMetric = prodsMetric[i][j];
            //            bestProd[i] = j;
            //        }
            //    }
            //}



            for (int moveIndex = 0; moveIndex < gameSettings.NumberOfMoves; moveIndex++)
            {
                //moveNumber = (int)formatter.Deserialize(inputStream);
                //bank = (Bank)formatter.Deserialize(inputStream);
                //words = (List<List<string>>)formatter.Deserialize(inputStream);
                //productionGroupNumber = (int)formatter.Deserialize(inputStream);

                Move move = new Move();

                //преобразуем слова к упрощенному виду, чтобы алогритм работал быстрее.
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                {
                    simpleWords.Add(new SimplifiedWord(word));
                }

                int groupNumber;

                if (prods[productionGroupNumber].Left == 'S')
                {
                    bank.addProduction(productionGroupNumber);
                    var word = new SimplifiedWord("S");
                    foreach (var prod in prods)
                        word.addNeterminal(prod.Left, 0);
                    var rez = findMove(word, move, bank, prods);
                    var word1 = new SuperSimplifiedWord("S", prods);
                    var rez1 = findMoveSecond(word1,move,bank,prods);
                }



                //while (true)
                //{
                //    List<List<int>> allowedWords = new List<List<int>>();
                //    foreach (var pr in prods)//находим слова допустимые для каждой продукции
                //    {
                //        allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                //        if (pr.Left == 'S')//если можно создать новое слово
                //            allowedWords.Last().Add(-1);
                //    }

                //    if (move.MovesCount > 0)//выбираем номер группы продукций
                //    {//выбираем продукцию из банка
                //        groupNumber = -1;
                //        double maxMetric = -1;
                //        for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                //            if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                //                if (netMetric[i] > maxMetric)
                //                {
                //                    maxMetric = netMetric[i];
                //                    groupNumber = i;
                //                }
                //        if (groupNumber == -1)
                //            break;//не нашлось продукций доступных к применению
                //    }
                //    else//если это первый ход - обязаны применить выпавшую продукцию
                //    {
                //        if (allowedWords[productionGroupNumber].Count == 0)
                //            break;
                //        groupNumber = productionGroupNumber;
                //    }

                //    SimplifiedProductionGroup prod = prods[groupNumber];
                //    int productionNumber = bestProd[groupNumber];//выбираем номер продукции в группе
                //    int wordNumber = -1;
                //    //выбираем слово. Выберем то слово, у кторого самая большая метрика
                //    {
                //        double maxMetric = -1;
                //        for (int i = 0; i < allowedWords[groupNumber].Count; ++i)
                //        {
                //            SimplifiedWord word;
                //            if (allowedWords[groupNumber][i] == -1)
                //                word = new SimplifiedWord("" + prods[groupNumber].Left);
                //            else
                //                word = simpleWords[allowedWords[groupNumber][i]];
                //            double metric = countWordMetric(word,
                //                gameSettings.RandomSettings,
                //                netMetric,
                //                prods);
                //            if (metric > maxMetric)
                //            {
                //                maxMetric = metric;
                //                wordNumber = i;
                //            }

                //        }
                //        wordNumber = allowedWords[groupNumber][wordNumber];
                //    }


                //    if (move.MovesCount >= 1)//удаляем продукцию из банка если надо
                //        bank.removeProduction(groupNumber);
                //    //теперь совершим этот ход
                //    move.addMove(wordNumber, groupNumber, productionNumber);


                //    if (wordNumber != -1)
                //    {
                //        SimplifiedWord word = simpleWords[wordNumber];
                //        word.neterminalsCount[prod.Left]--;
                //        word.terminals += prod.rights[productionNumber].terminals;
                //        foreach (var neterminal in prod.rights[productionNumber].neterminalsCount)
                //            word.addNeterminal(neterminal.Key, neterminal.Value);
                //    }
                //    else
                //    {
                //        SimplifiedWord word = new SimplifiedWord(prods[groupNumber].rights[productionNumber]);
                //        simpleWords.Add(word);
                //    }
                //}
                Console.Out.WriteLine(move);
            }
            return;
        }

        /// <summary>
        /// Алогоритм поиска максимальной выводимой строки из продукций уже находящихся в банке.
        /// </summary>
        /// <param name="oldWord"></param>
        /// <param name="currentMove"></param>
        /// <param name="bank"></param>
        /// <param name="prods"></param>
        /// <returns></returns>
        static KeyValuePair<int, string> findMoveSecond(SuperSimplifiedWord oldWord, Move currentMove, Bank bank, List<SimplifiedProductionGroup> prods)
        {
            KeyValuePair<int, string> bestWord = new KeyValuePair<int, string>(0, "");
            // = new KeyValuePair<int, string>(oldWord.terminals, currentMove.ToString()); ;
            KeyValuePair<int, string> tmpPair;
            SimplifiedWord tmpWord;
            SimplifiedProductionGroup prod;
            int prodsCount = prods.Count;
            bool found = false;
            for (int i = 0; i < oldWord.neterminalsCount.Length; ++i)
            {
                if (oldWord.neterminalsCount[i] > 0)
                {
                    found = true;
                    for (int prodIndex = 0; prodIndex < prodsCount; prodIndex++)
                    {
                        if (oldWord.CharToInt[prods[prodIndex].Left] == i && bank.getProductionCount(prodIndex) > 0)
                        {
                            prod = prods[prodIndex];
                            bank.removeProduction(prodIndex);
                            for (int rightIndex = 0; rightIndex < prod.RightSize; rightIndex++)
                            {
                                oldWord.neterminalsCount[oldWord.CharToInt[prod.Left]]--;
                                oldWord.terminals += prod.rights[rightIndex].terminals;
                                foreach (var net in prod.rights[rightIndex].neterminalsCount)
                                    oldWord.neterminalsCount[oldWord.CharToInt[net.Key]] += net.Value;


                                currentMove.addMove(0, prodIndex, rightIndex);
                                tmpPair = findMoveSecond(oldWord, currentMove, bank, prods);
                                if (bestWord.Key < tmpPair.Key)
                                    bestWord = tmpPair;


                                currentMove.popMove();
                                oldWord.neterminalsCount[oldWord.CharToInt[prod.Left]]++;
                                oldWord.terminals -= prod.rights[rightIndex].terminals;
                                foreach (var net in prod.rights[rightIndex].neterminalsCount)
                                    oldWord.neterminalsCount[oldWord.CharToInt[net.Key]] -= net.Value;
                            }
                            bank.addProduction(prodIndex);
                        }
                    }
                }
            }


            if (!found)
                return new KeyValuePair<int, string>(oldWord.terminals, currentMove.ToString());
            return bestWord;
        }

        static KeyValuePair<int, string> findMove(SimplifiedWord oldWord, Move currentMove, Bank bank, List<SimplifiedProductionGroup> prods)
        {
            KeyValuePair<int, string> bestWord = new KeyValuePair<int, string>(0, "");
            //  = new KeyValuePair<int, string>(oldWord.terminals, currentMove.ToString()); ;
            KeyValuePair<int, string> tmpPair;
            SimplifiedWord tmpWord;
            SimplifiedProductionGroup prod;
            bool found = false;
            for (int prodIndex = 0; prodIndex < prods.Count; ++prodIndex)
            {
                if (bank.getProductionCount(prodIndex) == 0) continue;

                prod = prods[prodIndex];
                if (oldWord.neterminalsCount[prod.Left] == 0) continue;
                found = true;
                bank.removeProduction(prodIndex);
                for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
                {
                    //tmpWord = new SimplifiedWord(oldWord);
                    oldWord.neterminalsCount[prod.Left]--;
                    oldWord.terminals += prod.rights[rightIndex].terminals;
                    foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                        oldWord.addNeterminal(neterminal.Key, neterminal.Value);

                    currentMove.addMove(0, prodIndex, rightIndex);
                    tmpPair = findMove(oldWord, currentMove, bank, prods);
                    if (bestWord.Key < tmpPair.Key)
                        bestWord = tmpPair;
                    currentMove.popMove();

                    oldWord.neterminalsCount[prod.Left]++;
                    oldWord.terminals -= prod.rights[rightIndex].terminals;
                    foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                        oldWord.addNeterminal(neterminal.Key, -neterminal.Value);
                }
                bank.addProduction(prodIndex);
            }
            if (!found)
                bestWord = new KeyValuePair<int, string>(oldWord.terminals, currentMove.ToString());
            return bestWord;
        }
    }
}
