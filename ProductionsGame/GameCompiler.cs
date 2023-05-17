using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductionsGameCore;

namespace ProductionsGame
{
    public abstract class GameCompiler
    {
        protected List<PlayerAdapter> players = new List<PlayerAdapter>();
        protected GameSettings gameSettings { get; private set; }
        protected RandomProvider randomProvider { get; private set; }
        protected Bank bank { get; private set; }
        public string LogFilename { get; private set; }
        protected StreamWriter log { get; private set; }

        public bool Active { get; private set; } = false;
        public bool Finished { get; private set; } = false;

        public int moveNumber { get; private set; } = 0;
        public int playerNumber { get; private set; } = 0;

        public GameCompiler(GameSettings gameSettings)
        {
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(@"./logs/"))//Создайм директорию для записи туда результатов
                Directory.CreateDirectory(@"./logs/");
            sb.Append(@"./logs/");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-"));
            sb.Append(Thread.CurrentThread.ManagedThreadId);
            sb.Append(".txt");
            this.LogFilename = sb.ToString();
            log = new StreamWriter(this.LogFilename);
            log.AutoFlush = false;
            this.gameSettings = gameSettings;
            bank = new Bank(gameSettings.ProductionsCount);
            randomProvider = new RandomProvider(gameSettings.RandomSettings);
            gameSettings.WriteToStream(log);
        }

        public GameCompiler(GameSettings gameSettings, string logFilename)
        {
            this.LogFilename = logFilename;
            log = new StreamWriter(this.LogFilename);
            log.AutoFlush = false;
            this.gameSettings = gameSettings;
            bank = new Bank(gameSettings.ProductionsCount);
            randomProvider = new RandomProvider(gameSettings.RandomSettings);
            gameSettings.WriteToStream(log);
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
            if (!(Active || Finished))
            {
                Active = true;
                for (moveNumber = 0; moveNumber < gameSettings.NumberOfMoves; ++moveNumber)
                {
                    for (playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
                    {
                        players[playerNumber].MakeMove(randomProvider.getRandom());
                        if (players[playerNumber].Finished)
                        {
                            Finished = true;
                            Active = false;
                            return;
                        }
                    }
                }
                Finished = true;
                Active = false;

                log.WriteLine("Результаты:");
                foreach (var player in players)
                {
                    log.WriteLine("Счёт игрока " + player.PlayerNumber + ": " + player.Score);
                }
                log.Close();
            }
        }

        public Move playOneMove()
        {
            if (!Finished)
            {
                Move move = players[playerNumber].MakeMove(randomProvider.getRandom());
                if (players[playerNumber].Finished)
                {
                    Finished = true;
                    Active = false;
                    return null;
                }
                ++playerNumber;
                if (playerNumber >= gameSettings.NumberOfPlayers)
                {
                    ++moveNumber;
                    playerNumber = 0;
                }
                if (moveNumber >= gameSettings.NumberOfMoves)
                {
                    Active = false;
                    Finished = true;
                    log.WriteLine("Результаты:");
                    foreach (var player in players)
                    {
                        log.WriteLine("Счёт игрока " + player.PlayerNumber + ": " + player.Score);
                    }
                    log.Close();
                }
                return move;
            }
            return null;
        }


        public int getPlayerScore(int index)
        {
            if (!(index >= 0 && index < players.Count))
                throw new IndexOutOfRangeException(
                String.Format("Индекс {0} был вне границ [0,{1}).", index, players.Count)
                );
            return players[index].Score;
        }
    }
}
