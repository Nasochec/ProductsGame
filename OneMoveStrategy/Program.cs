using ProductionsGameCore;
using ProductsGame;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace OneMoveStrategy
{
    internal class Program
    {
        //Максимальная глубина перебора
        const int maxDeep = 6;
        /// <summary>
        /// Идея стратегии - поскольку сделать перебор вариантов слишком затруднительно по времени, то возникла идея перебирать ходы с небольшой глубиной и выбрирать из получаемых выводов, вывод с лучшей метрикой, а после снова применять тот же перебор пока остаются доступные ходы.
        /// </summary>
        /// <param name="args"></param>
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

            
            List<double> wordsMetric = new List<double>();
            StringBuilder rez = new StringBuilder();
            for (int moveIndex = 0; moveIndex < gameSettings.NumberOfMoves; moveIndex++)
            {
                //считывам данные на каждом шагу
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);

                //преобразуем выводы к упрощенному виду, чтобы алогритм работал быстрее.
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                {
                    simpleWords.Add(new SimplifiedWord(word));
                }

                rez.Clear();

                //Посчитаем метрику всех выводов, потом будем её обновлять по ходу алгоритма.
                wordsMetric.Clear();
                for (int i = 0; i < simpleWords.Count; ++i)
                    wordsMetric.Add(StrategyUtilitiesClass.countWordMetric(simpleWords[i], gameSettings.RandomSettings, netMetric, prods));

                //Сначала мы обязаны применить именно выпавшую продукцию
                {
                    var prod = prods[productionGroupNumber];
                    List<int> allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left).ToList();

                    int maxIndex;
                    SimplifiedWord word = null;
                    {
                        double maxMetric = -1;
                        maxIndex = 0;
                        //находим вывод с максимальной метрикой
                        foreach (var index in allowedWords)
                        {
                            if (wordsMetric[index] > maxMetric)
                            {
                                maxMetric = wordsMetric[index];
                                maxIndex = index;
                                word = simpleWords[index];
                            }
                        }
                        //если можно создать новый вывод
                        if (prod.Left == 'S' &&
                            maxMetric < StrategyUtilitiesClass.countWordMetric(new SimplifiedWord("S"), gameSettings.RandomSettings, netMetric, prods))
                        {
                            maxIndex = -1;
                            word = new SimplifiedWord("S");
                        }
                        else if (maxMetric == -1)
                        {//Ни к одному выводу применить продукцию невозможно
                            Console.WriteLine();
                            continue;
                        }
                    }
                    Move move = new Move();
                    string bestMove = "", tmpMove;
                    double bestMetric = -1, tmpMetric, bestTerminals = 0, tmpTerminals;
                    //новый индекс вывода, если этот вывод создайтся на этом ходу (продукция S->) 
                    int newIndex = (maxIndex == -1 ? simpleWords.Count : maxIndex);
                    //Переберём варианты начальной продукции и выберем ту у которой метрика вывода получаемого с помощью findMove больше
                    word.neterminalsCount[prod.Left]--;
                    for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
                    {
                        word.terminals += prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            word.addNeterminal(neterminal.Key, neterminal.Value);

                        move.addMove(maxIndex, productionGroupNumber, rightIndex);
                        findMove(word, newIndex, bank, prods, gameSettings, netMetric, out tmpMove, out tmpMetric, out tmpTerminals, move);
                        if (bestMetric == -1 || tmpMetric > bestMetric || tmpMetric == bestMetric && tmpTerminals > bestTerminals)
                        {
                            bestMetric = tmpMetric;
                            bestMove = tmpMove;
                            bestTerminals = tmpTerminals;
                        }
                        move.popMove();

                        word.terminals -= prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            word.addNeterminal(neterminal.Key, -neterminal.Value);
                    }
                    word.neterminalsCount[prod.Left]++;

                    move = Move.FromString(bestMove);
                    bank.addProduction(productionGroupNumber);
                    applyMove(move, bank, word, prods);
                    rez.Append(move);
                }

                while (true)
                {
                    string bestMove;
                    double bestMetric, bestTerminals;
                    //найдём выводы к которым можно применить какие-либо продукции из банка
                    List<int> allowedIndexes = new List<int>();
                    for (int i = 0; i < simpleWords.Count; i++)
                    {
                        for (int prodIndex = 0; prodIndex < prods.Count; ++prodIndex)
                        {
                            var prod = prods[prodIndex];
                            //Если в банке есть продукция применимая к выводу, сохнаняем индекс вывода.
                            if (simpleWords[i].getNeterminal(prod.Left) > 0 && bank.getProductionCount(prodIndex) > 0)
                                allowedIndexes.Add(i);
                        }
                    }
                    if (allowedIndexes.Count == 0)//Выводов к которым можно применить продукции не осталось.
                        break;
                    int wordNumber = -1;
                    //Выберем тот вывод, у которого самая большая метрика.
                    {
                        double maxMetric = -1;
                        for (int i = 0; i < allowedIndexes.Count; ++i)
                        {
                            var word = simpleWords[allowedIndexes[i]];
                            double metric = StrategyUtilitiesClass.countWordMetric(word,
                                gameSettings.RandomSettings,
                                netMetric,
                                prods);
                            if (metric > maxMetric && metric != 1)
                            {
                                maxMetric = metric;
                                wordNumber = i;
                            }
                        }
                        wordNumber = allowedIndexes[wordNumber];
                    }
                    //находим лучший вывод
                    findMove(simpleWords[wordNumber], wordNumber, bank, prods, gameSettings, netMetric, out bestMove, out bestMetric, out bestTerminals);
                    Move move1 = Move.FromString(bestMove);
                    if (move1.MovesCount != 0)
                    {
                        wordsMetric[wordNumber] = bestMetric;
                        applyMove(move1, bank, simpleWords[wordNumber], prods);
                        rez.Append("," + bestMove);
                    }
                }
                Console.Out.WriteLine(rez.ToString());
            }
            return;
        }

        static void applyMove(Move move, Bank bank, SimplifiedWord word, List<SimplifiedProductionGroup> prods)
        {
            foreach (var m in move.getMoves())
            {
                bank.removeProduction(m.ProductionGroupNumber);
                var prod = prods[m.ProductionGroupNumber];
                word.neterminalsCount[prod.Left]--;
                word.terminals += prod.rights[m.ProductionNumber].terminals;
                foreach (var neterminal in prod.rights[m.ProductionNumber].neterminalsCount)
                    word.addNeterminal(neterminal.Key, -neterminal.Value);
            }
        }

        static void findMove(SimplifiedWord oldWord,
            int wordIndex,
            Bank bank,
            List<SimplifiedProductionGroup> prods,
            GameSettings settings,
            double[] netMetric,
            out string bestMove,
            out double bestMetric,
            out double bestTerminals,
            Move currentMove = null,
            int deep = 0)
        {

            bestMove = "";
            bestMetric = -1;
            bestTerminals = 0;
            bool found = false;
            if (deep < maxDeep)
            {
                if (currentMove == null)
                    currentMove = new Move();
                string bMove;
                double bMetric, bTerminals;
                SimplifiedProductionGroup prod;
                for (int prodIndex = 0; prodIndex < prods.Count; ++prodIndex)
                {
                    if (bank.getProductionCount(prodIndex) <= 0) continue;

                    prod = prods[prodIndex];
                    if (oldWord.getNeterminal(prod.Left) <= 0) continue;
                    found = true;
                    bank.removeProduction(prodIndex);
                    oldWord.neterminalsCount[prod.Left]--;
                    for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
                    {

                        oldWord.terminals += prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            oldWord.addNeterminal(neterminal.Key, neterminal.Value);

                        currentMove.addMove(wordIndex, prodIndex, rightIndex);
                        findMove(oldWord, wordIndex, bank, prods, settings, netMetric, out bMove, out bMetric, out bTerminals, currentMove, deep + 1);
                        if (bestMetric == -1 || bMetric > bestMetric || bMetric == bestMetric && bTerminals > bestTerminals)
                        {
                            bestMetric = bMetric;
                            bestMove = bMove;
                            bestTerminals = bTerminals;
                        }
                        currentMove.popMove();

                        oldWord.terminals -= prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            oldWord.addNeterminal(neterminal.Key, -neterminal.Value);
                    }
                    oldWord.neterminalsCount[prod.Left]++;
                    bank.addProduction(prodIndex);
                }
            }
            if (!found)
            {
                bestMetric = StrategyUtilitiesClass.countWordMetric(oldWord, settings.RandomSettings, netMetric, prods);
                bestMove = currentMove.ToString();
                bestTerminals = oldWord.terminals;
            }
        }
    }
}
