using ProductionsGameCore;
using ProductionsGame;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime;

namespace RandomStrategy
{
    internal class RandomStrategy
    {
        static void Main(string[] args)
        {
            Bank bank;
            GameSettings settings;
            int playerNumber;
            List<List<string>> words;
            int productionGroupNumber;
            int moveNumber;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream inputStream = Console.OpenStandardInput();

            //initialise start value for random
            Random random = new Random((int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId);

            //reading constant values
            settings = (GameSettings)formatter.Deserialize(inputStream);
            playerNumber = (int)formatter.Deserialize(inputStream);

            List<ProductionGroup> prods = settings.GetProductions().ToList();

            for (int moveIndex = 0; moveIndex < settings.NumberOfMoves; moveIndex++)
            {
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);

                Move move = new Move();

                int groupNumber;

                while (true)
                {
                    PrimaryMove primaryMove;
                    if (move.MovesCount == 0)//make first move
                        primaryMove = findFirstMove(random, words[playerNumber], prods, productionGroupNumber);
                    else//make move from bank
                        primaryMove = findMove(random, words[playerNumber], prods, bank);

                    //make this move
                    if (primaryMove != null)
                    {
                        if (move.MovesCount != 0)
                            bank.removeProduction(primaryMove.ProductionGroupNumber);
                        move.addMove(primaryMove);
                        applyMove(primaryMove, words[playerNumber], prods);
                    }
                    else
                        break;
                }
                Console.Out.WriteLine(move);
            }
            return;
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
