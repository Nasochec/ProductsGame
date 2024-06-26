﻿using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Strategies
{
    public class SearchStrategy : Strategy
    {
        //TODO add parameters for strategies for depth of search and for wieghts of combined strategy

        //TODO change it if this strategy works too long, or you want to make strategy better
        //max depth of search
        int maxDepth = 4;
        double[] netMetric;
        double[][] prodsMetric;


        /// <summary>
        /// Gets parameter depth - septh of recursion in search.
        /// </summary>
        /// <param name="parameters"></param>
        public SearchStrategy(Parameters parameters) : base()
        {
            if (parameters != null)
            {
                var param = parameters.getParameter("depth");
                if (param != null && param is IntParameter && (param as IntParameter).Value >= 0)
                    maxDepth = (param as IntParameter).Value;
            }
            this.GameSettingsChanged += beforeStart;
            Name = "Search Strategy "+maxDepth;
            ShortName = "SS"+maxDepth;
        }

        public static new Parameters getParameters() {
            Parameters searchParameters = new Parameters();
            searchParameters.addParameter(new IntParameter("depth", "Глубина перебора", 4));
            return searchParameters;
        }

        protected void beforeStart(object sender, EventArgs e) 
        {
            //count metric of broductions
            StrategyUtilitiesClass.countMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
        }

        public override Move makeMove(int playerNumber,
            int moveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank)
        {

            Move move = new Move();
            
            List<double> wordsMetric =new List<double>();
            //count metric of all words
            var simpleWords = simplifiedWords[playerNumber];
            wordsMetric.Clear();
            for (int i = 0; i < simpleWords.Count; ++i)
                wordsMetric.Add(StrategyUtilitiesClass.countWordMetric(simpleWords[i], GameSettings.RandomSettings, netMetric, simplifiedProductions));

            Move mov = findFirstMove(simpleWords, wordsMetric, bank, productionNumber);
            if (mov != null && mov.MovesCount != 0)
                move.addMove(mov);
            else
            {
                return move;
            }

            while (true)
            {
                Move move1 = findMove(simpleWords, wordsMetric, bank);
                if (move1 != null && move1.MovesCount != 0)
                    move.addMove(move1);
                else
                    break;
            }
            return move;
        }

        void searchMove(SimplifiedWord oldWord,
            int wordIndex,
            Bank bank,
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
                for (int prodIndex = 0; prodIndex < simplifiedProductions.Count; ++prodIndex)
                {
                    if (bank.getProductionCount(prodIndex) <= 0) continue;

                    prod = simplifiedProductions[prodIndex];
                    if (oldWord.getNonterminal(prod.Left) <= 0) continue;
                    found = true;
                    bank.removeProduction(prodIndex);
                    oldWord.addNonterminal(prod.Left, -1);
                    for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
                    {
                        oldWord.terminals += prod[rightIndex].terminals;
                        for(int nonterminal=0;nonterminal< prod[rightIndex].NonterminalsCount;++nonterminal)
                            oldWord.addNonterminal(nonterminal, prod[rightIndex][nonterminal]);
                        currentMove.addMove(wordIndex, prodIndex, rightIndex);
                        searchMove(oldWord, wordIndex, bank,
                            out bMove, out bMetric, out bTerminals, currentMove, depth + 1);
                        if (bestMetric == -1 || bMetric > bestMetric || bMetric == bestMetric && bTerminals > bestTerminals)
                        {
                            bestMetric = bMetric;
                            bestMove = bMove;
                            bestTerminals = bTerminals;
                        }
                        currentMove.popMove();

                        oldWord.terminals -= prod[rightIndex].terminals;
                        for (int nonterminal = 0; nonterminal < prod[rightIndex].NonterminalsCount; ++nonterminal)
                            oldWord.addNonterminal(nonterminal, -prod[rightIndex][nonterminal]);
                    }
                    oldWord.addNonterminal(prod.Left, 1);
                    bank.addProduction(prodIndex);
                }
            }
            if (!found)
            {
                bestMetric = StrategyUtilitiesClass.countWordMetric(oldWord, rs, netMetric, simplifiedProductions);
                bestMove = currentMove.ToString();
                bestTerminals = oldWord.terminals;
            }
        }

        Move findFirstMove(List<SimplifiedWord> simpleWords,
            List<double> wordsMetric, Bank bank, int productionGroupNumber
            )
        {

            var prod = simplifiedProductions[productionGroupNumber];
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
                if (simplifier.GetChar(prod.Left) == 'S' &&
                    maxMetric < StrategyUtilitiesClass.countWordMetric(simplifier.ConvertWord("S"), rs, netMetric, simplifiedProductions))
                {
                    maxIndex = -1;
                    word = simplifier.ConvertWord("S");
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
            word.addNonterminal(prod.Left, -1);
            for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
            {
                word.terminals += prod[rightIndex].terminals;
                for(int nonterminal = 0; nonterminal < prod[rightIndex].NonterminalsCount;++nonterminal)
                    word.addNonterminal(nonterminal, prod[rightIndex][nonterminal]);

                move.addMove(maxIndex, productionGroupNumber, rightIndex);
                searchMove(word,
                    newIndex,
                    bank,
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

                word.terminals -= prod[rightIndex].terminals;
                for (int nonterminal = 0; nonterminal < prod[rightIndex].NonterminalsCount; ++nonterminal)
                    word.addNonterminal(nonterminal,-prod[rightIndex][nonterminal]);
            }
            word.addNonterminal(prod.Left, 1);

            move = Move.FromString(bestMove);
            bank.addProduction(productionGroupNumber);
            StrategyUtilitiesClass.applyMove(move, bank, word, simplifiedProductions);
            return move;
        }

        Move findMove(List<SimplifiedWord> simpleWords,
            List<double> wordsMetric,
            Bank bank
            )
        {
            string bestMove;
            double bestMetric, bestTerminals;
            List<int> allowedIndexes = new List<int>();
            for (int i = 0; i < simpleWords.Count; i++)
            {
                for (int prodIndex = 0; prodIndex < simplifiedProductions.Count; ++prodIndex)
                {
                    var prod = simplifiedProductions[prodIndex];
                    if (simpleWords[i].getNonterminal(prod.Left) > 0 && bank.getProductionCount(prodIndex) > 0)
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
                        simplifiedProductions);
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
                out bestMove,
                out bestMetric,
                out bestTerminals);
            Move move1 = Move.FromString(bestMove);
            wordsMetric[wordNumber] = bestMetric;
            StrategyUtilitiesClass.applyMove(move1, bank, simpleWords[wordNumber], simplifiedProductions);
            return move1;
        }
    }
}
