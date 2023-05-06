using ProductionsGameCore;
using ProductsGame;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ShortWordsStrategy
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

            //Считываем данные постоянные на протяжении всей игры.
            gameSettings = (GameSettings)formatter.Deserialize(inputStream);
            playerNumber = (int)formatter.Deserialize(inputStream);


            List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

            //Преобразууем продукции к упрощенному виду, чтобы алгоритм работал быстрее.
            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();
            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            //Высчитываем метрику оценки продукций. При выборе продукции будем брать ту, у которой значение метрикик больше.
            double[] netMetric;
            double[][] prodsMetric;
            StrategyUtilitiesClass.countMetric(prods, gameSettings, out netMetric, out prodsMetric);

            //Найдём для каждой группы продукций лучшую продукцию.
            int[] bestProd;
            bestProd = new int[netMetric.Length];
            for (int i = 0; i < bestProd.Length; ++i)
            {
                double maxMetric = -1;
                for (int j = 0; j < prodsMetric[i].Length; ++j)
                {
                    if (maxMetric == -1 || prodsMetric[i][j] > maxMetric ||
                        prodsMetric[i][j] == maxMetric &&
                        prods[i].rights[j].terminals > prods[i].rights[bestProd[j]].terminals)
                    {
                        maxMetric = prodsMetric[i][j];
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

                //преобразуем выводы к упрощенному виду, чтобы алогритм работал быстрее.
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                    simpleWords.Add(new SimplifiedWord(word));

                int groupNumber;

                while (true)
                {
                    List<List<int>> allowedWords = new List<List<int>>();
                    foreach (var pr in prods)//находим выводы допустимые для каждой продукции
                    {
                        allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                        if (pr.Left == 'S')//если можно создать новый вывод
                            allowedWords.Last().Add(-1);
                    }

                    if (move.MovesCount > 0)//выбираем номер группы продукций
                    {//выбираем продукцию из банка
                        groupNumber = -1;
                        double maxMetric = -1;
                        for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                            if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                                if (netMetric[i] > maxMetric)
                                {
                                    maxMetric = netMetric[i];
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
                    //Выберем тот вывод, у кторого самая большая метрика
                    {
                        double maxMetric = -1;
                        for (int i = 0; i < allowedWords[groupNumber].Count; ++i)
                        {
                            SimplifiedWord word;
                            if (allowedWords[groupNumber][i] == -1)
                                word = new SimplifiedWord("" + prods[groupNumber].Left);
                            else
                                word = simpleWords[allowedWords[groupNumber][i]];
                            double metric = StrategyUtilitiesClass.countWordMetric(word,
                                gameSettings.RandomSettings,
                                netMetric,
                                prods);//для оптимизации, возможно стоит хранить метрики слов и обновлять их по ходу алгоритма
                            if (metric > maxMetric)
                            {
                                maxMetric = metric;
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
    }
}
