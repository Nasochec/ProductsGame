using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGame
{
    public class Game
    {
        public GameSettings GameSettings { get; private set; }
        public RandomSettings RandomSettings { get  { return GameSettings.RandomSettings; } }
        public IEnumerable<ProductionGroup> Productions { get { return GameSettings.GetProductions(); } }
        private List<SimplifiedProductionGroup> SimplifiedProductions;
        private List<Strategy> players;
        protected RandomProvider RandomProvider { get; private set; }
        public Bank Bank { get; private set; }

        public int MoveNumber { get; private set; }
        public int ActivePlayer { get;private set; }

        public List<List<string>> words;
        public List<List<SimplifiedWord>> simplifiedWords;
        List<int> scores;
        private bool scoresOutdated;
        private int CurrentWinner;

        public bool Active { get; private set; } = false;
        public bool Finished { get; private set; } = false;

        public Game(GameSettings gameSettings,IEnumerable<Strategy> players) { 
            GameSettings = gameSettings;
            if (players.Count() != 2)
                throw new ArgumentException("There are must be two players in game.");
            this.players = players.ToList();
            MoveNumber = 0;
            ActivePlayer = 0;
            Bank = new Bank(GameSettings.ProductionsCount);
            RandomProvider = new RandomProvider(GameSettings.RandomSettings);
            words = new List<List<string>> { new List<string>() , new List<string>() };
            simplifiedWords = new List<List<SimplifiedWord>> { new List<SimplifiedWord>(),new List<SimplifiedWord>()};
            scores = new List<int> { 0, 0 };
        }


        public void play()
        {
            if (!(Active || Finished))
            {
                Active = true;
                for (MoveNumber = 0; MoveNumber < GameSettings.NumberOfMoves; ++MoveNumber)
                {
                    for (ActivePlayer = 0; MoveNumber < 2; ++ActivePlayer)
                    {
                        Move move = players[ActivePlayer].makeMove(ActivePlayer, MoveNumber, RandomProvider.getRandom(), words, simplifiedWords, Bank);
                        if (move == null)
                        {
                            Finished = true;
                            Active = false;
                            return;
                        }
                        scoresOutdated = true;
                    }
                }
                Finished = true;
                Active = false;
            }
        }

        public Move playOneMove() {
            if (!Finished)
            {

                //TODO chatch exception, log it
                //try
                //{ 
                    Move move = players[ActivePlayer]//TODO - send copies + apply moves
                        .makeMove(ActivePlayer, MoveNumber, RandomProvider.getRandom(), words, simplifiedWords, Bank);
                    if (move==null)
                    {
                        Finished = true;
                        Active = false;
                        return null;
                    }
                    scoresOutdated = true;
                    ++ActivePlayer;
                    if (ActivePlayer >= 2)
                    {
                        ++MoveNumber;
                        ActivePlayer = 0;
                    }
                    if (MoveNumber >= GameSettings.NumberOfMoves)
                    {
                        Active = false;
                        Finished = true;
                        //log.WriteLine("Результаты:");
                        //foreach (var player in players)
                        //{
                        //    log.WriteLine("Счёт игрока " + player.PlayerNumber + ": " + player.Score);
                        //}
                        //log.Close();
                    }
                    return move;
                //} catch (Exception e)
                //{
                //    Finished = true;
                    
                //}
                
            }
            return null;
        }

        public List<string> getPlayers() {
            return players.Select(x=> x.ToString()).ToList();
        }

        public int getWinner() {
            if(scoresOutdated)
                countScores();
            return CurrentWinner;
        }

        public List<int> getScores() {
            if (scoresOutdated)
                countScores();
            return scores.ToList();
        }

        private void countScores() { 
            scoresOutdated = false;
            for(int i=0;i<2;++i) {
                scores[i]  = simplifiedWords[i].Select(word=>word.getScore()).Sum();
            }
            CurrentWinner = scores[0] > scores[1] ? 0 : 1;
        }
    }
}
