﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductionsGameCore;

namespace ProductsGame
{
    public class GameCompiler
    {
        private List<PlayerAdapter> players = new List<PlayerAdapter>();
        private GameSettings gameSettings;
        private RandomProvider randomProvider;
        private Bank bank;
        private string logFilename;

        public GameCompiler(GameSettings gameSettings, IEnumerable<string> playersFilenames)
        {
            if (gameSettings.NumberOfPlayers != 2 || !gameSettings.IsBankShare || gameSettings.ProductionsCount != 12) {
                throw new ArgumentException("Указанная конфигурация игры пока не поддерживается");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(@"./logs/");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-"));
            sb.Append(Thread.CurrentThread.ManagedThreadId);
            sb.Append(".txt");
            this.logFilename = sb.ToString();
            this.gameSettings = gameSettings;
            bank = new Bank(gameSettings.ProductionsCount);
            randomProvider = new RandomProvider(gameSettings.RandomSettings);
            if (playersFilenames.Count() != gameSettings.NumberOfPlayers)
                throw new ArgumentException("Количество игроков должно быть равно количеству игроков указанному в конфигурации игры.");
            var playerFilenameEnumerator = playersFilenames.GetEnumerator();
            for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
            {
                playerFilenameEnumerator.MoveNext();
                players.Add(new ExeSerializationPlayerAdapter(playerNumber, this, logFilename, playerFilenameEnumerator.Current));
            }
            gameSettings.WriteToFile(logFilename);
            using (StreamWriter log = new StreamWriter(logFilename, true))
            {
                log.WriteLine();
                int index = 0;
                foreach (var player in playersFilenames)
                {
                    index++;
                    log.WriteLine("Player " + index + ": " + player);
                }
            }
        }

        public Bank getBank()
        {
            return bank;
        }

        public GameSettings GetGameSettings()
        {
            return gameSettings;
        }

        public List<List<string>> getPlayersWords()
        {
            List<List<string>> words = new List<List<string>>();
            foreach (var player in players)
            {
                words.Add(player.getWords().ToList());
            }
            return words;
        }

        public void play()
        {
            for (int moveNumber = 0; moveNumber < gameSettings.NumberOfMoves; ++moveNumber)
            {
                for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
                {
                    players[playerNumber].MakeMove(randomProvider.getRandom());
                    if (players[playerNumber].Finished)
                        return;//TODO maybe do something
                }
            }

            using (StreamWriter log = new StreamWriter(logFilename, true))
            {
                foreach (var player in players)
                {
                    log.WriteLine("Player " + player.PlayerNumber + " score: " + player.Score);
                }
            }
        }
    }
}
