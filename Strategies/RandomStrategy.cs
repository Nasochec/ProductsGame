using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Strategies
{
    public class RandomStrategy: Strategy
    {
        private Random random;


        RandomStrategy():base() {
            random = new Random((int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId);
        }

        public override Move makeMove(int productionNumber)
        {
            Move move = new Move();

            //int groupNumber;

            while (true)
            {
                PrimaryMove primaryMove;
                if (move.MovesCount == 0)//make first move
                    primaryMove = findFirstMove(random, words[PlayerNumber], prods, productionNumber);
                else//make move from bank
                    primaryMove = findMove(random, words[PlayerNumber], prods, Bank);

                //make this move
                if (primaryMove != null)
                {
                    if (move.MovesCount != 0)
                        Bank.removeProduction(primaryMove.ProductionGroupNumber);
                    move.addMove(primaryMove);
                    applyMove(primaryMove, words[PlayerNumber], prods);
                }
                else
                    break;
            }
            return move;
        }


        static PrimaryMove findFirstMove(Random random, List<string> words, List<ProductionGroup> prods, int productionGroupNumber)
        {
            List<int> allowedWords = new List<int>();

            var prod = prods[productionGroupNumber];
            allowedWords = StrategyUtilitiesClass.findMatches(words, prod.Left);
            if (prod.Left == 'S')//if can create new word
                allowedWords.Add(-1);

            if (allowedWords.Count == 0)
                return null;//not found production

            int productionNumber = random.Next(prod.RightSize);
            int wordnumber = random.Next(allowedWords.Count);//select word
            wordnumber = allowedWords[wordnumber];

            return new PrimaryMove(wordnumber, productionGroupNumber, productionNumber);
        }

        static PrimaryMove findMove(Random random, List<string> words, List<ProductionGroup> prods, Bank bank)
        {
            List<List<int>> allowedWords = new List<List<int>>();
            int productionGroupNumber;
            foreach (var pr in prods)//find words allowed for productions
            {
                allowedWords.Add(StrategyUtilitiesClass.findMatches(words, pr.Left));
                if (pr.Left == 'S')//if can create new word
                    allowedWords.Last().Add(-1);
            }

            List<int> allowedGroupIndexes = new List<int>();
            for (int i = 0; i < prods.Count; ++i)
                if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                    allowedGroupIndexes.Add(i);
            if (allowedGroupIndexes.Count == 0)
                return null;//not found production
            productionGroupNumber = allowedGroupIndexes[random.Next(allowedGroupIndexes.Count)];

            ProductionGroup prod = prods[productionGroupNumber];
            int productionNumber = random.Next(prod.RightSize);

            int wordnumber = random.Next(allowedWords[productionGroupNumber].Count);//select word

            wordnumber = allowedWords[productionGroupNumber][wordnumber];
            return new PrimaryMove(wordnumber, productionGroupNumber, productionNumber);
        }

        static void applyMove(PrimaryMove move, List<string> words, List<ProductionGroup> prods)
        {
            ProductionGroup prod = prods[move.ProductionGroupNumber];
            if (move.WordNumber != -1)
            {
                string word = words[move.WordNumber];
                int letterIndex = StrategyUtilitiesClass.isHaveLetter(word, prod.Left);
                string newWord = word.Substring(0, letterIndex) +
                         prod.getRightAt(move.ProductionNumber) +
                         word.Substring(letterIndex + 1, word.Length - letterIndex - 1);
                words[move.WordNumber] = newWord;
            }
            else
            {
                string newWord = prod.getRightAt(move.ProductionNumber);
                words.Add(newWord);
            }
        }
    }
}
