using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsGame
{
    public class GameCompiler
    {
        private List<PlayerAdapter> players = new List<PlayerAdapter>();
        private GameSettings gameSettings;
        private Bank bank;

        GameCompiler(string filename) { 
            gameSettings = GameSettings.ReadFromFile(filename);
            bank = new Bank(gameSettings.GetProductions());
            for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
                players.Add(new ExePlayerAdapter(gameSettings, bank,);

        }

        //public IEnumerable<string> getPlayerWords() { 
             
        //    foreach (PlayerAdapter playerAdapter in players) {
        //}



    }
}
