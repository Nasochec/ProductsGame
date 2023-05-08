using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsGameLauncher
{
    internal class GameResult
    {
        public string filename { get; private set; }
        public GameSettings gameSettings { get; private set; }
        public List<string> playersFilenames = new List<string>();
        public List<string> shortPlayersFilanames = new List<string>();
        public List<int> playersScores = new List<int>();

        public GameResult(string filename)
        {
            this.filename = filename;
            //readConfig();
            StringBuilder sb = new StringBuilder();
            StreamReader fs = new StreamReader(filename);

            while (true)
            {
                string s = fs.ReadLine();
                sb.Append(s);
                if (s.Equals("</GameSettings>"))
                    break;
            }

            string confString = sb.ToString();
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(confString));
            gameSettings = GameSettings.ReadFromStream(stream);

            string p1 = fs.ReadLine();
            string p2 = fs.ReadLine();

            playersFilenames.Add(p1);
            playersFilenames.Add(p2);

            shortPlayersFilanames.Add(p1.Split('/').Last());
            shortPlayersFilanames.Add(p2.Split('/').Last());

            while (true)
            {
                string s = fs.ReadLine();
                if (s == null) break;
                if (s.StartsWith("Cчёт игрока ", StringComparison.Ordinal)) {
                    playersScores.Add(int.Parse(s.Split(':')[1]));
                }
            }


        }

    }
}
