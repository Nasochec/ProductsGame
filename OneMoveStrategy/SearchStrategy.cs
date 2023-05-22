using ProductionsGameCore;
using ProductionsGame;
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

namespace SearchStrategy
{
    internal class SearchStrategy
    {
        //TODO change it if this strategy works too long, or you want to make strategy better
        //max deep of search
        static int maxDeep = 4;
        /// <summary>
        /// Идея стратегии - поскольку сделать перебор вариантов слишком затруднительно по времени, то возникла идея перебирать ходы с небольшой глубиной и выбрирать из получаемых выводов, вывод с лучшей метрикой, а после снова применять тот же перебор пока остаются доступные ходы.
        /// </summary>
        /// <param name="args"> Передаётся глубина перебора - по-умолчанию установлена 4, не может быть отрицательным и не может быть больше 8.</param>
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {//reading maxDeep from parameters
                int t = -1;
                if (int.TryParse(args[0], out t) && t >= 1 && t <= 7)
                    maxDeep = t;
            }

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
            double[] netMetric;
            double[][] prodsMetric;
            StrategyUtilitiesClass.countMetric(prods, gameSettings, out netMetric, out prodsMetric);


            List<double> wordsMetric = new List<double>();
            StringBuilder rez = new StringBuilder();
            for (int moveIndex = 0; moveIndex < gameSettings.NumberOfMoves; moveIndex++)
            {
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);

                //get simplified form of words
                simpleWords.Clear();
                foreach (var word in words[playerNumber])
                {
                    simpleWords.Add(new SimplifiedWord(word));
                }

                rez.Clear();

                //count metric of all words
                wordsMetric.Clear();
                for (int i = 0; i < simpleWords.Count; ++i)
                    wordsMetric.Add(StrategyUtilitiesClass.countWordMetric(simpleWords[i], gameSettings.RandomSettings, netMetric, prods));

                Move mov = findFirstMove(prods, simpleWords, wordsMetric, gameSettings.RandomSettings, bank, productionGroupNumber, netMetric);
                if (mov != null && mov.MovesCount != 0)
                    rez.Append(mov.ToString());
                else {
                    Console.WriteLine();
                    continue;
                }

                while (true)
                {
                    Move move1 = findMove(prods,
                        simpleWords,
                        wordsMetric,
                        gameSettings.RandomSettings,
                        bank,
                        netMetric
                        );
                    if (move1 != null && move1.MovesCount != 0)
                    {
                        rez.Append("," + move1.ToString());
                    }
                    else
                        break;
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
                word.addNeterminal(prod.Left, -1);
                word.terminals += prod.rights[m.ProductionNumber].terminals;
                foreach (var neterminal in prod.rights[m.ProductionNumber].neterminalsCount)
                    word.addNeterminal(neterminal.Key, -neterminal.Value);
            }
        }

        static void searchMove(SimplifiedWord oldWord,
            int wordIndex,
            Bank bank,
            List<SimplifiedProductionGroup> prods,
            RandomSettings rs,
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
                    oldWord.addNeterminal(prod.Left, -1);
                    for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
                    {

                        oldWord.terminals += prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            oldWord.addNeterminal(neterminal.Key, neterminal.Value);

                        currentMove.addMove(wordIndex, prodIndex, rightIndex);
                        searchMove(oldWord, wordIndex, bank, prods, rs, netMetric, 
                            out bMove, out bMetric, out bTerminals, currentMove, deep + 1);
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
                    oldWord.addNeterminal(prod.Left, 1);
                    bank.addProduction(prodIndex);
                }
            }
            if (!found)
            {
                bestMetric = StrategyUtilitiesClass.countWordMetric(oldWord, rs, netMetric, prods);
                bestMove = currentMove.ToString();
                bestTerminals = oldWord.terminals;
            }
        }

        static Move findFirstMove(List<SimplifiedProductionGroup> prods,
            List<SimplifiedWord> simpleWords,
            List<double> wordsMetric,
            RandomSettings rs,
            Bank bank,
            int productionGroupNumber,
            double[] netMetric)
        {

            var prod = prods[productionGroupNumber];
            //fond allowed words
            List<int> allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left).ToList();

            int maxIndex;
            SimplifiedWord word = null;
            {
                double maxMetric = -1;
                maxIndex = 0;
                //find word with minimum metric
                foreach (var index in allowedWords)
                {
                    if (wordsMetric[index] > maxMetric)
                    {
                        maxMetric = wordsMetric[index];
                        maxIndex = index;
                        word = simpleWords[index];
                    }
                }
                //if can create new word
                if (prod.Left == 'S' &&
                    maxMetric < StrategyUtilitiesClass.countWordMetric(new SimplifiedWord("S"), rs, netMetric, prods))
                {
                    maxIndex = -1;
                    word = new SimplifiedWord("S");
                }
                else if (maxMetric == -1)
                    return null;
            }
            Move move = new Move();
            string bestMove = "", tmpMove;
            double bestMetric = -1, tmpMetric, bestTerminals = 0, tmpTerminals;
            //if we can create new word (production S->) 
            int newIndex = (maxIndex == -1 ? simpleWords.Count : maxIndex);
            //senumerate all productions what we can apply to the word
            word.addNeterminal(prod.Left, -1);
            for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
            {
                word.terminals += prod.rights[rightIndex].terminals;
                foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                    word.addNeterminal(neterminal.Key, neterminal.Value);

                move.addMove(maxIndex, productionGroupNumber, rightIndex);
                searchMove(word,
                    newIndex,
                    bank,
                    prods,
                    rs,
                    netMetric,
                    out tmpMove,
                    out tmpMetric,
                    out tmpTerminals,
                    move);
                if (bestMetric == -1 || tmpMetric > bestMetric
                    || tmpMetric == bestMetric && tmpTerminals > bestTerminals)
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
            word.addNeterminal(prod.Left, 1);

            move = Move.FromString(bestMove);
            bank.addProduction(productionGroupNumber);
            applyMove(move, bank, word, prods);
            return move;
        }

        static Move findMove(List<SimplifiedProductionGroup> prods,
            List<SimplifiedWord> simpleWords,
            List<double> wordsMetric,
            RandomSettings rs,
            Bank bank,
            double[] netMetric)
        {
            string bestMove;
            double bestMetric, bestTerminals;
            List<int> allowedIndexes = new List<int>();
            for (int i = 0; i < simpleWords.Count; i++)
            {
                for (int prodIndex = 0; prodIndex < prods.Count; ++prodIndex)
                {
                    var prod = prods[prodIndex];
                    if (simpleWords[i].getNeterminal(prod.Left) > 0 && bank.getProductionCount(prodIndex) > 0)
                        allowedIndexes.Add(i);
                }
            }
            if (allowedIndexes.Count == 0)//no words found
                return null;
            int wordNumber = -1;
            //select word with best metric
            {
                double maxMetric = -1;
                for (int i = 0; i < allowedIndexes.Count; ++i)
                {
                    var word = simpleWords[allowedIndexes[i]];
                    double metric = StrategyUtilitiesClass.countWordMetric(word,
                        rs,
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
            //find best move
            searchMove(simpleWords[wordNumber],
                wordNumber,
                bank,
                prods,
                rs,
                netMetric,
                out bestMove,
                out bestMetric,
                out bestTerminals);
            Move move1 = Move.FromString(bestMove);
            wordsMetric[wordNumber] = bestMetric;
            applyMove(move1, bank, simpleWords[wordNumber], prods);
            return move1;
        }
    }
}
