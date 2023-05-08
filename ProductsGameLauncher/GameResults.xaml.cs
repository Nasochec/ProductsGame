using ProductionsGameCore;
using ProductsGame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProductsGameLauncher
{
    /// <summary>
    /// Interaction logic for GameResults.xaml
    /// </summary>
    public partial class GameResults : Window
    {
        //private GameCompiler compiler;
        private GameSettings gameSettings;
        private int numberOfRounds;
        //файлы стратегий игроков
        List<string> playersFilenames;

        //файлы результатов раундов игркоков
        List<string> resultFilenames;


        //Результаты игроков в раундах.
        //firstPlayerScores[i,j] - очки первого игрока в раунже где игрок i был первым, а j вторым
        //secondPlayerScores[i,j] - очки второго игрока в раунже где игрок i был первым, а j вторым
        List<int>[,] firstPlayerScores;
        List<int>[,] secondPlayerScores;

        bool isTournament;


        public GameResults()
        {
            InitializeComponent();
        }

        public GameResults(GameSettings gs, IEnumerable<string> tournamentPlayersFilenames, int numberOfRounds, bool isTournament)
        {
            InitializeComponent();
            if (gs.NumberOfPlayers != 2)
            {
                throw new NotImplementedException("Число игрков отличное от 2 пока не поддерживается.");
            }
            this.numberOfRounds = numberOfRounds;
            gameSettings = gs;
            playersFilenames = tournamentPlayersFilenames.ToList();
            resultFilenames = new List<string>();
            this.isTournament = isTournament;
            play();
        }

        public void play()
        {
            const int maxActiveThreads = 40;
            List<Thread> activeThreads = new List<Thread>();
            int finishedThreads = 0;


            int round = 0;
            int firsrIndex = 0, secondIndex = 0;
            int playersNumber = playersFilenames.Count;
            int allRounds = playersNumber * playersNumber * numberOfRounds;
            if (!isTournament)
                secondIndex = 1;

            firstPlayerScores = new List<int>[playersFilenames.Count, playersFilenames.Count];
            secondPlayerScores = new List<int>[playersFilenames.Count, playersFilenames.Count];
            for (int i = 0; i < playersNumber; ++i)
                for (int j = 0; j < playersNumber; ++j)
                {
                    firstPlayerScores[i, j] = new List<int>();
                    secondPlayerScores[i, j] = new List<int>();
                }
            while (round < numberOfRounds || activeThreads.Count != 0)
            {
                //запускаем новые раунды, пока не достигнем макс. доступное количество потоков, или не запуустим все раунды
                while (activeThreads.Count < maxActiveThreads && round < numberOfRounds)
                {
                    Thread newGCThread = new Thread(new ThreadStart(() =>
                    {
                        int f = firsrIndex, s = secondIndex;
                        ExeSerializationGameCompiler gc =
                        new ExeSerializationGameCompiler(
                            gameSettings,
                            new string[] { playersFilenames[f], playersFilenames[s] }
                        );
                        //lock (resultFilenames)
                        //{
                        resultFilenames.Add(gc.LogFilename);
                        //}
                        gc.play();
                        //lock (firstPlayerScores[f, s])
                        //{
                        //    firstPlayerScores[f, s].Add(gc.getPlayerScore(0));
                        //}
                        //lock (secondPlayerScores[f, s])
                        //{
                        //    firstPlayerScores[f, s].Add(gc.getPlayerScore(1));
                        //}
                    }
                    ));
                    newGCThread.Start();
                    activeThreads.Add(newGCThread);
                    if (isTournament)
                    {
                        secondIndex++;
                        if (secondIndex >= playersNumber)
                        {
                            firsrIndex++;
                            secondIndex = 0;
                            if (firsrIndex >= playersNumber)
                            {
                                round++;
                                firsrIndex = 0;
                                secondIndex = 0;
                            }
                            continue;
                        }
                    }
                    else
                        round++;
                }
                //освобождаем потоки/дожидаемся окончания работы всех потоков.
                for (int i = 0; i < activeThreads.Count;)
                {
                    Thread thread = activeThreads[i];
                    if (!thread.IsAlive)
                    {
                        activeThreads.RemoveAt(i);
                        ++finishedThreads;
                        //backgroundWorker.ReportProgress(finishedThreads);
                        continue;
                    }
                    ++i;
                }
            }
        }

        public void fillGameResults()
        {

        }
    }
}
