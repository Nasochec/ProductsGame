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

namespace RandomStrategy
{
    internal class Program
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

            //зададим начальное значения генератора случайных чисел
            Random random = new Random((int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId);
            
            //Считываем данные постоянные на протяжении всей игры.
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
                    List<List<int>> allowedWords = new List<List<int>>();
                    foreach (var pr in prods)//находим выводы допустимые для каждой продукции
                    {
                        allowedWords.Add(StrategyUtilitiesClass.findMatches(words[playerNumber], pr.Left));
                        if (pr.Left == 'S')//если можно создать новый вывод
                            allowedWords.Last().Add(-1);
                    }

                    if (move.MovesCount > 0)//выбираем номер группы продукций
                    {//выбираем продукцию из банка
                        List<int> allowedGroupIndexes = new List<int>();
                        for (int i = 0; i < settings.ProductionsCount; ++i)
                            if (allowedWords[i].Count > 0 && bank.getProductionCount(i) > 0)
                                allowedGroupIndexes.Add(i);
                        if (allowedGroupIndexes.Count == 0)
                            break;//не нашлось продукций доступных к применению
                        groupNumber = allowedGroupIndexes[random.Next(allowedGroupIndexes.Count)];
                    }
                    else//если это первый ход - обязаны применить выпавшую продукцию
                    {
                        if (allowedWords[productionGroupNumber].Count == 0)
                            break;//нет выводов к которым можно применить первую продукцию
                        groupNumber = productionGroupNumber;
                    }
                    ProductionGroup prod = settings.getProductionGroup(groupNumber);
                    int productionNumber = random.Next(prod.RightSize);//выбираем номер продукции в группе

                    int wordnumber = random.Next(allowedWords[groupNumber].Count);//выбираем вывод
                    if (move.MovesCount >= 1)
                        bank.removeProduction(groupNumber);
                    wordnumber = allowedWords[groupNumber][wordnumber];
                    //теперь совершим этот ход
                    move.addMove(wordnumber, groupNumber, productionNumber);
                    if (wordnumber != -1)
                    {
                        string word = words[playerNumber][wordnumber];
                        int letterIndex = StrategyUtilitiesClass.isHaveLetter(word, prod.Left);
                        string newWord = word.Substring(0, letterIndex) +
                                 prod.getRightAt(productionNumber) +
                                 word.Substring(letterIndex + 1, word.Length - letterIndex - 1);
                        words[playerNumber][wordnumber] = newWord;
                    }
                    else
                    {
                        string newWord = prod.getRightAt(productionNumber);
                        words[playerNumber].Add(newWord);
                    }
                }
                Console.Out.WriteLine(move);
            }
            return;
        }
    }
}
