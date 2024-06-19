using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Strategies
{
    public class RandomStrategy : Strategy
    {
        protected Random random;


        public RandomStrategy() : base()
        {
            Name = "Random Strategy";
            ShortName = "RS";
            random = new Random((int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId);
        }
        /// <summary>
        /// Counts weights of production groups.
        /// override this method for new strategy
        /// </summary>
        protected virtual List<double> getGroupsWeights(List<int> indexes) { 
            return indexes.Select((index)=>1d).ToList();
        }
        /// <summary>
        /// Counts weights of productions in production groups.
        /// </summary>
        protected virtual List<double> getProductionsWeights(int index) {
            int prods = productions[index].RightSize;
            List<double> weights = new List<double>();
            for (int i = 0; i < prods; ++i)
                weights.Add(1d);
            return weights;
        }
        /// <summary>
        /// Counts weights of words.
        /// </summary>
        protected virtual List<double> getWordsWeights(List<SimplifiedWord> words) {
            return words.Select((index) => 1d).ToList();  
        }

        private int getRand(List<double> weights)
        {
            double sum = 0;
            foreach (double weight in weights)
                sum += weight;
            double rand = random.NextDouble() * sum;
            int i = 0;
            foreach (double weight in weights)
            {
                rand -= weight;
                if (rand <= 0)
                    return i;
                i++;
            }
            return 0;
        }

        public override Move makeMove(int playerNumber,
            int moveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank)
        {
            Move move = new Move();


            while (true)
            {//TODO use simplified forms
                PrimaryMove primaryMove;
                if (move.MovesCount == 0)//make first move
                    primaryMove = findFirstMove(simplifiedWords[playerNumber], productionNumber);
                else//make move from bank
                    primaryMove = findMove(simplifiedWords[playerNumber], bank);

                //make this move
                if (primaryMove != null)
                {
                    if (move.MovesCount != 0)
                        bank.removeProduction(primaryMove.ProductionGroupNumber);
                    move.addMove(primaryMove);
                    StrategyUtilitiesClass.applyMove(primaryMove, simplifiedWords[playerNumber], simplifiedProductions);
                }
                else
                    break;
            }
            return move;
        }


        private PrimaryMove findFirstMove(List<SimplifiedWord> words, int productionGroupNumber)
        {
            List<int> allowedWordsIndexes = new List<int>();
            List<SimplifiedWord> allowedWords;


            var prod = simplifiedProductions[productionGroupNumber];
            allowedWordsIndexes = StrategyUtilitiesClass.findMatches(words, prod.Left);
            allowedWords = allowedWordsIndexes.Select((index) => words[index]).ToList();
            if (simplifier.GetChar(prod.Left) == 'S')
            {//if can create new word
                allowedWordsIndexes.Add(-1);
                allowedWords.Add(simplifier.ConvertWord("S"));
            }

            if (allowedWordsIndexes.Count == 0)
                return null;//not found production

            int productionNumber = getRand(getProductionsWeights(productionGroupNumber));
            int wordnumber = getRand(getWordsWeights(allowedWords));
            wordnumber = allowedWordsIndexes[wordnumber];//select word

            return new PrimaryMove(wordnumber, productionGroupNumber, productionNumber);
        }

        private PrimaryMove findMove(List<SimplifiedWord> words, Bank bank)
        {
            List<List<int>> allowedWordsIndexes = new List<List<int>>();
            List<List<SimplifiedWord>> allowedWords = new List<List<SimplifiedWord>>();

            int productionGroupNumber;
            foreach (var pr in simplifiedProductions)//find words allowed for productions
            {
                allowedWordsIndexes.Add(StrategyUtilitiesClass.findMatches(words, pr.Left));
                allowedWords.Add(allowedWordsIndexes.Last().Select((index) => words[index]).ToList());
                if (simplifier.GetChar(pr.Left) == 'S')
                {//if can create new word
                    allowedWordsIndexes.Last().Add(-1);
                    allowedWords.Last().Add(simplifier.ConvertWord("S"));
                }
            }

            List<int> allowedGroupIndexes = new List<int>();
            for (int i = 0; i < productions.Count; ++i)
                if (allowedWordsIndexes[i].Count > 0 && bank.getProductionCount(i) > 0)
                    allowedGroupIndexes.Add(i);
            if (allowedGroupIndexes.Count == 0)
                return null;//not found production
            productionGroupNumber = getRand(getGroupsWeights(allowedGroupIndexes));
            productionGroupNumber = allowedGroupIndexes[productionGroupNumber];

            int productionNumber = getRand(getProductionsWeights(productionGroupNumber));
            int wordnumber = getRand(getWordsWeights(allowedWords[productionGroupNumber]));
            wordnumber = allowedWordsIndexes[productionGroupNumber][wordnumber];//select word

            return new PrimaryMove(wordnumber, productionGroupNumber, productionNumber);
        }
    }
}
