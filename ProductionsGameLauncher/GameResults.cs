using ProductionsGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProductionsGameLauncher
{
    internal class GameResults
    {
        public ObservableCollection<FileGameResult> results;
        public Dictionary<string, int> playersToInt;
        public List<string> playersNames;
        public List<string> shortPlayersNames;
        public int[,] firstPlayerScore;
        public int[,] secondPlayerScore;
        public int[,] gamesCount;

        public int[,] firstPlayerWin;
        public int[,] secondPlayerWin;

        public double[,] firstPlayerMeanScore;
        public double[,] secondPlayerMeanScore;

        public GameResults():this(new List<string>())
        {   
        }

        public GameResults(List<string> resultFilenames)
        {
            results = new ObservableCollection<FileGameResult>();
            addResults(resultFilenames);
            fillGameResults();
        }

        public void addResults(IEnumerable<string> filenames)
        {
            foreach (var fname in filenames)
                addResults(fname);
        }

        public void addResults(string filename)
        {
            results.Add(new FileGameResult(filename));
        }

        public void fillGameResults()
        {
            List<FileGameResult> results = new List<FileGameResult>();
            playersToInt = new Dictionary<string, int>();
            playersNames = new List<string>();
            shortPlayersNames = new List<string>();
            //находим всех игроков
            foreach (var s in this.results)
            {
                FileGameResult rez = s;
                if (rez.playersScores.Count != 2 || rez.playersNames.Count != 2 || rez.shortPlayersFilanames.Count!=2)
                    continue;
                results.Add(rez);
                for (int i = 0; i < 2; ++i)//находим всех игроков встречавшихся в логирующих файла
                    if (!playersToInt.ContainsKey(rez.playersNames[i]))
                    {
                        playersToInt.Add(rez.playersNames[i], playersToInt.Count);
                        playersNames.Add(rez.playersNames[i]);
                        shortPlayersNames.Add(rez.shortPlayersFilanames[i]);
                    }
            }

            int playersCount = playersToInt.Count;
            firstPlayerScore = new int[playersCount, playersCount];
            secondPlayerScore = new int[playersCount, playersCount];
            firstPlayerWin = new int[playersCount, playersCount];
            secondPlayerWin = new int[playersCount, playersCount];
            gamesCount = new int[playersCount, playersCount];
            foreach (var rez in results)
            {
                int f = playersToInt[rez.playersNames[0]];
                int s = playersToInt[rez.playersNames[1]];
                ++gamesCount[f, s];
                firstPlayerScore[f, s] += rez.playersScores[0];
                secondPlayerScore[f, s] += rez.playersScores[1];
                if (rez.winner == Game.Winner.First)
                    firstPlayerWin[f, s]++;
                else if (rez.winner == Game.Winner.Second)
                    secondPlayerWin[f, s]++;

            }
            firstPlayerMeanScore = new double[playersCount, playersCount];
            secondPlayerMeanScore = new double[playersCount, playersCount];

            for (int i = 0; i < playersCount; ++i)
                for (int j = 0; j < playersCount; ++j)
                {
                    firstPlayerMeanScore[i, j] = (double)firstPlayerScore[i, j] / (double)gamesCount[i, j];
                    secondPlayerMeanScore[i, j] = (double)secondPlayerScore[i, j] / (double)gamesCount[i, j];
                }
        }

        public void writeToFile(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                int sz = playersNames.Count;
                {
                    sw.WriteLine(@"\begin{center}");
                    sw.Write(@"\begin{tabular}{");
                    for (int i = 0; i <= sz; i++)
                        sw.Write("| c ");
                    sw.WriteLine("|}");
                    sw.WriteLine(@"\hline");
                    for (int i = 0; i < sz; i++)
                        sw.Write(string.Format("& {0}", shortPlayersNames[i]));
                    sw.WriteLine(@"\\");
                    sw.WriteLine(@"\hline");
                    for (int i = 0; i < sz; i++)
                    {
                        sw.Write(shortPlayersNames[i]);
                        for (int j = 0; j < sz; j++)
                        {
                            sw.Write(" & ");
                            sw.Write((int)firstPlayerMeanScore[i, j]);
                            sw.Write("/");
                            sw.Write((int)secondPlayerMeanScore[i, j]);
                        }
                        sw.WriteLine(@"\\");
                        sw.WriteLine(@"\hline");
                    }
                    sw.WriteLine(@"\end{tabular}");
                    sw.WriteLine(@"\end{center}");
                }
                {
                    sw.WriteLine(@"\begin{center}");
                    sw.Write(@"\begin{tabular}{");
                    for (int i = 0; i <= sz; i++)
                        sw.Write("| c ");
                    sw.WriteLine("|}");
                    sw.WriteLine(@"\hline");
                    for (int i = 0; i < sz; i++)
                        sw.Write(string.Format("& {0}", shortPlayersNames[i]));
                    sw.WriteLine(@"\\");
                    sw.WriteLine(@"\hline");
                    for (int i = 0; i < sz; i++)
                    {
                        sw.Write(shortPlayersNames[i]);
                        for (int j = 0; j < sz; j++)
                        {
                            sw.Write(" & ");
                            sw.Write(firstPlayerWin[i, j]);
                            sw.Write("/");
                            sw.Write(secondPlayerWin[i, j]);
                        }
                        sw.WriteLine(@"\\");
                        sw.WriteLine(@"\hline");
                    }
                    sw.WriteLine(@"\end{tabular}");
                    sw.WriteLine(@"\end{center}");
                }
            }
        }
    }
}
