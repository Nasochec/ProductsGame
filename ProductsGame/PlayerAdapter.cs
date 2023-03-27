using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace ProductsGame
{
    public abstract class PlayerAdapter: IWords
    {
        public int MoveNumber
        {
            get;
            private set;
        }
        private List<string> words;
        protected int MyNumber { get; private set; }
        protected GameSettings Settings { get; private set; }
        protected Bank Bank { get; private set; }
        protected List<IWords> PlayersWords { get; private set; }

        //protected GameCompiler GameCompiler { get; private set; }

        public PlayerAdapter(int number, GameSettings settings, Bank bank, IEnumerable<IWords> playersWords)
        {
            MoveNumber = 0;
            MyNumber = number;
            this.words = new List<string>();
            Settings = settings;
            Bank = bank;
            PlayersWords = playersWords.ToList();
        }

        /// <summary>
        /// Метод применяющий выбранную продукцию из выбранной группы к выбранному слову.*/
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="firstMoveProductionGroupNumber"></param>
        /// <exception cref="Exception"> если введён некорректный ход. </exception>
        private void applyMove(Move moves, int firstMoveProductionGroupNumber)
        {
            bool isfirst = true;

            foreach (var move in moves.getMoves())
            {
                int productionGroupNumber = move.ProductionGroupNumber,
                    productionNumber = move.ProductionNumber,
                    wordNumber = move.WordNumber;
                ProductionGroup production = Settings.getProductionGroup(productionGroupNumber);
                char left = production.Left;
                string right = production.getRightAt(productionNumber);

                if (isfirst)
                {//Проверка что указана верная продукция 
                    if (firstMoveProductionGroupNumber != productionGroupNumber)
                        throw new Exception(String.Format("Wrong move. Production group must be {0} but was {1}.", firstMoveProductionGroupNumber, productionNumber));
                }
                else
                {//Проверка что указаная продукция есть в банке
                    if (Bank.getProductionCount(production) == 0)
                        throw new Exception(String.Format("Wrong move. Bank doesn't contain production group {0}.", productionGroupNumber));
                }
                if (wordNumber >= 0 && wordNumber < words.Count)
                {
                    string word = words[wordNumber];
                    int index = word.IndexOf(left);
                    if (index == -1)
                    {
                        throw new Exception(String.Format("Wrong move. Word {0} doesn't contain letter {1}", word, left));
                    }
                    else
                    {//B->cDe : aBf-> acDef
                        string newWord = word.Substring(0, index - 1) +
                            right +
                            word.Substring(index + 1, word.Length - index - 1);
                        words[index] = newWord;
                    }
                }
                else
                {//Применяем продукцию к новому слову
                    if (left == 'S')
                    {
                        words.Add(right);
                    }
                }
                if (!isfirst)  //удаляем продукцию из банка
                    Bank.removeProduction(production);
                isfirst = false;
            }
            if (isfirst)//осталось истиной -> ни одна продукция не была применена, добавить её в банк
            {
                ProductionGroup production = Settings.getProductionGroup(firstMoveProductionGroupNumber);
                Bank.addProduction(production);
            }

            {// проверить что применены все мозможные продукции
                int productionNumber = 0;
                foreach (var production in Settings.GetProductions())
                {
                    productionNumber++;
                    char left = production.Left;
                    foreach (var word in words)
                    {
                        if (word.IndexOf(left) != -1)
                            throw new Exception(String.Format("The production group {0} can be aplied to word {1}.", productionNumber, word));
                    }
                }
            }
        }

        public void MakeMove(int productionGroupNumber, Bank bank)
        {
            Move move = this.CalculateMove(productionGroupNumber, bank.getProductions());
            //TODO add Logging
            applyMove(move, productionGroupNumber);
            //TODO catch exception and write it to logs
        }

        /// <summary>
        /// Метод который надо поределить при написании стратерии.
        /// </summary>
        /// <param name="productionGroupNumber"></param>
        /// <param name="bank"></param>
        /// <returns></returns>
        protected abstract Move CalculateMove(int productionGroupNumber, IEnumerable<KeyValuePair<ProductionGroup, int>> bank);

        public IEnumerable<string> getWords() {
            return words.AsEnumerable();
        }
    
    }
}
