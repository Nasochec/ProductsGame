﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using ProductionsGameCore;

namespace ProductsGame
{
    public abstract class PlayerAdapter
    {
        public int MoveNumber
        {
            get;
            private set;
        }
        private List<string> words;
        public int PlayerNumber { get; private set; }
        protected GameCompiler GameCompiler { get; private set; }
        protected GameSettings Settings { get; private set; }
        protected Bank Bank { get; private set; }
        private string logFilename;
        public int Score { get; private set; }
        public bool Finished { get; private set; }

        public PlayerAdapter(int number, GameCompiler gameCompiler, string logFilename)
        {
            MoveNumber = 0;
            PlayerNumber = number;
            this.words = new List<string>();
            GameCompiler = gameCompiler;
            Settings = GameCompiler.GetGameSettings();
            Bank = GameCompiler.getBank();
            this.logFilename = logFilename;
            Finished = false;
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
                    if (Bank.getProductionCount(productionGroupNumber) == 0)
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
                        string newWord = word.Substring(0, index) +
                            right +
                            word.Substring(index + 1, word.Length - index - 1);
                        words[wordNumber] = newWord;
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
                    Bank.removeProduction(productionGroupNumber);
                isfirst = false;
            }
            if (isfirst)//осталось истиной -> ни одна продукция не была применена, добавить её в 
                Bank.addProduction(firstMoveProductionGroupNumber);

            {// проверить что применены все возможные продукции
                int productionNumber = 0;
                foreach (var production in Settings.GetProductions())
                {
                    if (Bank.getProductionCount(productionNumber) <= 0)
                        continue;
                    char left = production.Left;
                    productionNumber++;
                    foreach (var word in words)
                    {
                        if (word.IndexOf(left) != -1)
                            throw new Exception(String.Format("The production group {0} can be aplied to word {1}.", productionNumber, word));
                    }
                }
            }
            MoveNumber++;
        }

        public void MakeMove(int productionGroupNumber)
        {
            if (Finished)
                return;
            StreamWriter log = new StreamWriter(logFilename, true);
            log.AutoFlush = false;
            Move move = null;
            try
            {
                move = this.CalculateMove(productionGroupNumber);
                applyMove(move, productionGroupNumber);
                log.WriteLine("Production number:{0}", productionGroupNumber);
                log.WriteLine("Move: {0}, Player: {1}", MoveNumber, PlayerNumber);
                log.WriteLine("Move: {0}", move.ToString());
                log.WriteLine("Bank: {0}", Bank.ToString());
                List<List<string>> words = GameCompiler.getPlayersWords();
                for (int playerNumber = 0; playerNumber < words.Count; ++playerNumber)
                {
                    log.Write("Player {0} words:", playerNumber);
                    foreach (var word in words[playerNumber])
                    {
                        log.Write(" " + word);
                    }
                    log.WriteLine();
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("ERROR");
                log.WriteLine("Программа заранее завершила работу из-за игрока" + PlayerNumber + ":" + ex.Message);
                log.WriteLine("Конфигурация, на которой произошла ошибка: ");
                log.WriteLine("Production number:{0}", productionGroupNumber);
                log.WriteLine("Move: {0}, Player: {1}", MoveNumber, PlayerNumber);
                if(move != null)
                    log.WriteLine("Move: {0}", move.ToString());
                log.WriteLine("Bank: {0}", Bank.ToString());
                List<List<string>> words = GameCompiler.getPlayersWords();
                for (int playerNumber = 0; playerNumber < words.Count; ++playerNumber)
                {
                    log.Write("Player {0} words:", playerNumber);
                    foreach (var word in words[playerNumber])
                    {
                        log.Write(" " + word);
                    }
                    log.WriteLine();
                }
                Finished = true;
            }
            finally
            {
                log.Flush();
                log.Close();
            }

            //TODO catch exception and write it to logs
            if (MoveNumber == Settings.NumberOfMoves || Finished)//в конце игры - подсчитываем очки
                calculateScore();
        }

        /// <summary>
        /// Метод который надо поределить при написании стратерии.
        /// </summary>
        /// <param name="productionGroupNumber"></param>
        /// <param name="bank"></param>
        /// <returns></returns>
        protected abstract Move CalculateMove(int productionGroupNumber);

        public IEnumerable<string> getWords()
        {
            return words.AsEnumerable();
        }

        private void calculateScore()
        {//TODO возможно можно переделать сделав отдельное хранилище сентенциальных форм
            Score = 0;
            foreach (var word in words)
            {
                if (!Regex.IsMatch(word, "[A-Z]"))//проверка что слово - сентенция
                {
                    Score += word.Length;
                    Score += 3;
                }
            }

        }

    }
}
