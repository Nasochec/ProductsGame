using ProductionsGameCore;
using ProductionsGame;
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
    internal class ShortWordsStrategy
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
                    PrimaryMove primaryMove;
                    if (move.MovesCount == 0)
                        primaryMove = findFirstMove(prods, simpleWords, gameSettings.RandomSettings, bank, netMetric, bestProd, productionGroupNumber);
                    else
                        primaryMove = findMove(prods, simpleWords, gameSettings.RandomSettings, bank, netMetric, bestProd);
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

        static PrimaryMove findMove(List<SimplifiedProductionGroup> prods, List<SimplifiedWord> simpleWords, RandomSettings rs, Bank bank, double[] netMetric, int[] bestProd)
        {
            int prosuctionGroupNumber;
            List<List<int>> allowedWords = new List<List<int>>();
            foreach (var pr in prods)//находим выводы допустимые для каждой продукции
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(simpleWords, pr.Left));
                if (pr.Left == 'S')//если можно создать новый вывод
                    allowedWords.Last().Add(-1);
            }

            //выбираем продукцию из банка
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
                    return null;//не нашлось продукций доступных к применению
            }

            SimplifiedProductionGroup prod = prods[prosuctionGroupNumber];
            int productionNumber = bestProd[prosuctionGroupNumber];//выбираем номер продукции в группе
            int wordNumber = -1;
            //Выберем тот вывод, у кторого самая большая метрика
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
                        prods);//для оптимизации, возможно стоит хранить метрики слов и обновлять их по ходу алгоритма
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

        static PrimaryMove findFirstMove(List<SimplifiedProductionGroup> prods, List<SimplifiedWord> simpleWords, RandomSettings rs, Bank bank, double[] netMetric, int[] bestProd, int productionGroupNumber)
        {
            int groupNumber;
            var prod = prods[productionGroupNumber];
            List<int> allowedWords = new List<int>();

            allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left);
            if (prod.Left == 'S')//если можно создать новый вывод
                allowedWords.Add(-1);

            if (allowedWords.Count == 0)
                return null;
            groupNumber = productionGroupNumber;

            int productionNumber = bestProd[groupNumber];//выбираем номер продукции в группе
            int wordNumber = -1;
            //Выберем тот вывод, у кторого самая большая метрика
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

        static void applyMove(PrimaryMove move, List<SimplifiedWord> simpleWords, List<SimplifiedProductionGroup> prods)
        {
            var prod = prods[move.ProductionGroupNumber];
            if (move.WordNumber != -1)
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
