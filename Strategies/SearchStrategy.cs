using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Strategies
{
    public class SearchStrategy : Strategy
    {
        //TODO add parameters for strategies for depth of search and for wieghts of combined strategy

        //TODO change it if this strategy works too long, or you want to make strategy better
        //max depth of search
        int maxDepth = 4;
        List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();
        double[] netMetric;
        double[][] prodsMetric;
        List<SimplifiedProductionGroup> simplifiedProds = new List<SimplifiedProductionGroup>();
        List<double> wordsMetric = new List<double>();

        /// <summary>
        /// Gets parameter depth - septh of recursion in search.
        /// </summary>
        /// <param name="prameters"></param>
        public SearchStrategy(Parameters prameters) : base("Search Strategy") {
            var param = prameters.getParameter("depth");
            if(param !=null && param.Value>=0)
                maxDepth = param.Value;
        }

        protected override void beforeStart()
        {

            //get the simplified form of productions

            for (int i = 0; i < Settings.ProductionsCount; ++i)
                simplifiedProds.Add(new SimplifiedProductionGroup(Settings.getProductionGroup(i)));

            //count metric of broductions

            StrategyUtilitiesClass.countMetric(simplifiedProds, Settings, out netMetric, out prodsMetric);
        }

        public override Move makeMove(int productionNumber, int MoveNumber, List<List<string>> words, Bank bank)
        {
            //get simplified form of words
            simpleWords.Clear();
            foreach (var word in words[PlayerNumber])
            {
                simpleWords.Add(new SimplifiedWord(word));
            }

            Move move = new Move();

            //count metric of all words
            wordsMetric.Clear();
            for (int i = 0; i < simpleWords.Count; ++i)
                wordsMetric.Add(StrategyUtilitiesClass.countWordMetric(simpleWords[i], Settings.RandomSettings, netMetric, simplifiedProds));

            Move mov = findFirstMove(simplifiedProds, simpleWords, wordsMetric, Settings.RandomSettings, bank, productionNumber, netMetric);
            if (mov != null && mov.MovesCount != 0)
                move.addMove(mov);
            else
            {
                return move;
            }

            while (true)
            {
                Move move1 = findMove(simplifiedProds,
                    simpleWords,
                    wordsMetric,
                    Settings.RandomSettings,
                    bank,
                    netMetric
                    );
                if (move1 != null && move1.MovesCount != 0)
                {
                    move.addMove(move1);
                }
                else
                    break;
            }
            return move;
        }

        void searchMove(SimplifiedWord oldWord,
            int wordIndex,
            Bank bank,
            List<SimplifiedProductionGroup> prods,
            RandomSettings rs,
            double[] netMetric,
            out string bestMove,
            out double bestMetric,
            out double bestTerminals,
            Move currentMove = null,
            int depth = 0)
        {

            bestMove = "";
            bestMetric = -1;
            bestTerminals = 0;
            bool found = false;
            if (depth < maxDepth)
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
                            out bMove, out bMetric, out bTerminals, currentMove, depth + 1);
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

        Move findFirstMove(List<SimplifiedProductionGroup> prods,
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
            StrategyUtilitiesClass.applyMove(move, bank, word, prods);
            return move;
        }

        Move findMove(List<SimplifiedProductionGroup> prods,
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
            StrategyUtilitiesClass.applyMove(move1, bank, simpleWords[wordNumber], prods);
            return move1;
        }
    }
}
