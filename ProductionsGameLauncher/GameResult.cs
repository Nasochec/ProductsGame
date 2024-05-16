using ProductionsGame;
using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionsGameLauncher
{
    internal abstract class GameResult
    {
        public List<string> playersNames = new List<string>();
        public List<string> shortPlayersFilanames;
        public List<int> playersScores = new List<int>();
        public Game.Winner winner { get; protected set; }

        public override string ToString()
        {
            if (playersNames.Count == 2 && playersScores.Count == 2)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(playersNames[0]);
                sb.Append(" vs ");
                sb.Append(playersNames[1]);
                sb.Append(":");
                sb.Append(playersScores[0]);
                sb.Append('-');
                sb.Append(playersScores[1]);
                return sb.ToString();
            }
            else return "Неверный формат файла.";
        }
    }
    /// <summary>
    /// Класс, используемый чтобы из файлов с резальтатами игр получить данные об составе игроков и их очках.
    /// </summary>
    internal class FileGameResult:GameResult
    {
        public string filename { get; private set; }
        //public GameSettings gameSettings { get; private set; }

        public FileGameResult(string filename)
        {
            try
            {
                this.filename = filename;
                //StringBuilder sb = new StringBuilder();
                StreamReader fs = new StreamReader(filename);

                while (true)
                {
                    string s = fs.ReadLine();
                    //sb.Append(s);
                    if (s.Equals("</GameSettings>"))
                        break;
                }

                //string confString = sb.ToString();
                //Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(confString));
                //gameSettings = GameSettings.ReadFromStream(stream);

                string p1 = fs.ReadLine();
                string p2 = fs.ReadLine();

                playersNames.Add(p1.Split(':').Last());
                playersNames.Add(p2.Split(':').Last());
                shortPlayersFilanames = playersNames.Select((name) => 
                    string.Concat(name.Where(c => c >= 'A' && c <= 'Z'))
                    ).ToList();

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
                if(playersScores.Count>0)
                    if (playersScores[0] > playersScores[1])
                        winner = Game.Winner.First;
                    else if (playersScores[1] > playersScores[0])
                        winner = Game.Winner.Second;
                    else
                        winner = Game.Winner.Draw;
                else
                    winner = Game.Winner.Draw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Файл результатов игры в неверном формате.");
            }
        }
    }
    /// <summary>
    /// Класс, используемый чтобы из файлов с резальтатами игр получить данные об составе игроков и их очках.
    /// </summary>
    internal class CompilerGameResult:GameResult
    {
        public CompilerGameResult(Game game)
        {
            if (!(game.state == Game.State.Failed || game.state ==Game.State.Failed))
                throw new ArgumentException("Game is't finished.");
            playersNames = game.getPlayers();
            shortPlayersFilanames = playersNames.Select((name) =>
                   string.Concat(name.Where(c => c >= 'A' && c <= 'Z'))
                   ).ToList();
            playersScores = game.getScores();
            winner = game.getWinner();
        }
    }
}
