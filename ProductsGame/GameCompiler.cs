using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public GameCompiler(string filename)
        {
            gameSettings = GameSettings.ReadFromFile(filename);
            bank = new Bank(gameSettings.ProductionsCount);
            randomProvider = new RandomProvider(gameSettings.RandomSettings);
            //for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
            //    players.Add(new ExeSerializationPlayerAdapter(playerNumber, this,));
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
    }
}
