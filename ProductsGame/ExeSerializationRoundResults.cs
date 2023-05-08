using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductionsGameCore;

namespace ProductsGameLauncher
{
    internal class ExeSerializationRoundResults
    {
        public int RoundNumber { get; private set; }
        GameSettings gameSettings;
        string filename;
        List<string> playersFilenames;
        List<int> playersScores;

        public ExeSerializationRoundResults(GameSettings gameSettings/*, string filename, IEnumerable<string> playersFilenames, IEnumerable<int> playersScores*/)
        {
            //this.gameSettings = gameSettings;
            this.filename = filename;
            //this.playersFilenames = playersFilenames.ToList();
            //this.playersScores = playersScores.ToList();


        }

        //public RoundResults()
    }
}
