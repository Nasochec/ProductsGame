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

            //Считываем данные постоянные на протяжении всей игры.
            gameSettings = (GameSettings)formatter.Deserialize(inputStream);
            playerNumber = (int)formatter.Deserialize(inputStream);

            List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

            //Преобразууем продукции к упрощенному виду, чтобы алгоритм работал быстрее.
            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();
            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            //Высчитываем метрику оценки продукций. При выборе продукции будем брать ту, у которой значение метрикик больше.
            int[] netMetric;
            int[][] prodsMetric;
            StrategyUtilitiesClass.countStupidMetric(prods, out netMetric, out prodsMetric);

            //Найдём для каждой группы продукций лучшую продукцию.
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

                //преобразуем выводы к упрощенному виду, чтобы алогритм работал быстрее.
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                    simpleWords.Add(new SimplifiedWord(word));

                int groupNumber;

                while (true)
                {
                    PrimaryMove primaryMove;
                    if (move.MovesCount == 0)//Совершаем первый ход
                        primaryMove = findFirstMove(prods, simpleWords, bank, netMetric, bestProd, productionGroupNumber);
                    else//совершаем ход используя банк
                        primaryMove = findMove(prods, simpleWords, bank, netMetric, bestProd);

                    if (primaryMove != null)
                    {
                        if (move.MovesCount != 0)//удаляем продукцию из банка если надо
                            bank.removeProduction(primaryMove.ProductionGroupNumber);
                        //теперь совершим этот ход
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
            foreach (var pr in prods)//находим выводы допустимые для каждой продукции
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                if (pr.Left == 'S')//если можно создать новый вывод
                    allowedWords.Last().Add(-1);
            }

            //выбираем продукцию из банка
            {//находим группу продукций с меньшей метрикой
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
                    return null;//не нашлось продукций доступных к применению
            }

            SimplifiedProductionGroup prod = prods[groupNumber];
            int productionNumber = bestProd[groupNumber];//выбираем номер продукции в группе
            int wordNumber = -1;
            //Выберем тот вывод, у кторого самая маленькая метрика
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
            //находим выводы допустимые для продукции
            allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left);
            if (prod.Left == 'S')//если можно создать новый вывод
                allowedWords.Add(-1);

            if (allowedWords.Count == 0)
                return null;
            groupNumber = productionGroupNumber;

            int productionNumber = bestProd[groupNumber];//выбираем номер продукции в группе
            int wordNumber = -1;
            //Выберем тот вывод, у кторого самая маленькая метрика
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
