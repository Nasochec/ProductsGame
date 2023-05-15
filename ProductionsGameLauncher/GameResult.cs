using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Класс, используемый чтобы из файлов с резальтатами игр получить данные об составе игроков и их очках.
    /// </summary>
    internal class GameResult
    {
        public string filename { get; private set; }
        public GameSettings gameSettings { get; private set; }
        public List<string> playersFilenames = new List<string>();
        public List<string> shortPlayersFilanames = new List<string>();
        public List<int> playersScores = new List<int>();

        public GameResult(string filename)
        {
            try
            {
                this.filename = filename;
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

                playersFilenames.Add(p1.Split(':').Last());
                playersFilenames.Add(p2.Split(':').Last());

                shortPlayersFilanames.Add(p1.Split(':').Last().Split('\\', '/').Last());
                shortPlayersFilanames.Add(p2.Split(':').Last().Split('\\', '/').Last());

                while (true)
                {
                    string s = fs.ReadLine();
                    if (s == null)
                        break;
                    if (s.Equals("Результаты:"))
                    {
                        string r1 = fs.ReadLine();
                        string r2 = fs.ReadLine();
                        playersScores.Add(int.Parse(r1.Split(':')[1]));
                        playersScores.Add(int.Parse(r2.Split(':')[1]));
                    }
                }
                fs.Close();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Файл результатов игры в неверном формате.");
            }
        }
    }
}
