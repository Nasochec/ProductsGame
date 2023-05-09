using Microsoft.Win32;
using ProductionsGameCore;
using ProductionsGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Interaction logic for GameResults.xaml
    /// </summary>
    public partial class GameResults : Window
    {
        private GameSettings gameSettings;
        private int numberOfRounds;
        //файлы стратегий игроков
        List<string> playersFilenames;
        List<string> shortPlayersFilenames;

        //файлы результатов раундов игркоков
        //List<string> resultFilenames = new List<string>();

        ObservableCollection<string> resultFilenames;

        Dictionary<string, int> playersToInt;



        //в firstPlayerScores[i,j] - сумма очков игрока i, в игре где игрок i - ходит первым, а j - вторым
        //в secondPlayerScores[i,j] - сумма очков игрока j, в игре где игрок i - ходит первым, а j - вторым
        //в gamesCount[i,j] - кличество игр, где игрок i - ходит первым, а j - вторым
        int[,] firstPlayerScore;
        int[,] secondPlayerScore;
        int[,] gamesCount;

        //в firstPlayerMeanScores[i,j] - среднее арифметическое очков игрока i, в игре где игрок i - ходит первым, а j - вторым
        //в secondPlayerMeanScores[i,j] - среднее арифметическое очков игрока j, в игре где игрок i - ходит первым, а j - вторым
        double[,] firstPlayerMeanScore;
        double[,] secondPlayerMeanScore;


        public GameResults()
        {
            InitializeComponent();
            resultFilenames = new ObservableCollection<string>();
            SelectedFilesListBox.ItemsSource = resultFilenames;
        }

        public GameResults(List<string> resultFilenames)
        {
            InitializeComponent();
            this.resultFilenames = new ObservableCollection<string>(resultFilenames);
            fillGameResults();
            fillDataGrid();
            SelectedFilesListBox.ItemsSource = resultFilenames;
        }

        //public GameResults(GameSettings gs, IEnumerable<string> tournamentPlayersFilenames, int numberOfRounds, bool isTournament)
        //{
        //    InitializeComponent();
        //    if (gs.NumberOfPlayers != 2)
        //    {
        //        throw new NotImplementedException("Число игрков отличное от 2 пока не поддерживается.");
        //    }
        //    this.numberOfRounds = numberOfRounds;
        //    gameSettings = gs;
        //    playersFilenames = tournamentPlayersFilenames.ToList();
        //    resultFilenames = new List<string>();
        //    this.isTournament = isTournament;
        //    play();
        //}

        //public void play()
        //{
        //    const int maxActiveThreads = 40;
        //    List<Thread> activeThreads = new List<Thread>();
        //    int finishedThreads = 0;


        //    int round = 0;
        //    int firsrIndex = 0, secondIndex = 0;
        //    int playersNumber = playersFilenames.Count;
        //    int allRounds = playersNumber * playersNumber * numberOfRounds;
        //    if (!isTournament)
        //        secondIndex = 1;

        //    firstPlayerScores = new List<int>[playersFilenames.Count, playersFilenames.Count];
        //    secondPlayerScores = new List<int>[playersFilenames.Count, playersFilenames.Count];
        //    for (int i = 0; i < playersNumber; ++i)
        //        for (int j = 0; j < playersNumber; ++j)
        //        {
        //            firstPlayerScores[i, j] = new List<int>();
        //            secondPlayerScores[i, j] = new List<int>();
        //        }
        //    while (round < numberOfRounds || activeThreads.Count != 0)
        //    {
        //        //запускаем новые раунды, пока не достигнем макс. доступное количество потоков, или не запуустим все раунды
        //        while (activeThreads.Count < maxActiveThreads && round < numberOfRounds)
        //        {
        //            Thread newGCThread = new Thread(new ThreadStart(() =>
        //            {
        //                int f = firsrIndex, s = secondIndex;
        //                ExeSerializationGameCompiler gc =
        //                new ExeSerializationGameCompiler(
        //                    gameSettings,
        //                    new string[] { playersFilenames[f], playersFilenames[s] }
        //                );
        //                //lock (resultFilenames)
        //                //{
        //                resultFilenames.Add(gc.LogFilename);
        //                //}
        //                gc.play();
        //                //lock (firstPlayerScores[f, s])
        //                //{
        //                //    firstPlayerScores[f, s].Add(gc.getPlayerScore(0));
        //                //}
        //                //lock (secondPlayerScores[f, s])
        //                //{
        //                //    firstPlayerScores[f, s].Add(gc.getPlayerScore(1));
        //                //}
        //            }
        //            ));
        //            newGCThread.Start();
        //            activeThreads.Add(newGCThread);
        //            if (isTournament)
        //            {
        //                secondIndex++;
        //                if (secondIndex >= playersNumber)
        //                {
        //                    firsrIndex++;
        //                    secondIndex = 0;
        //                    if (firsrIndex >= playersNumber)
        //                    {
        //                        round++;
        //                        firsrIndex = 0;
        //                        secondIndex = 0;
        //                    }
        //                    continue;
        //                }
        //            }
        //            else
        //                round++;
        //        }
        //        //освобождаем потоки/дожидаемся окончания работы всех потоков.
        //        for (int i = 0; i < activeThreads.Count;)
        //        {
        //            Thread thread = activeThreads[i];
        //            if (!thread.IsAlive)
        //            {
        //                activeThreads.RemoveAt(i);
        //                ++finishedThreads;
        //                continue;
        //            }
        //            ++i;
        //        }
        //    }
        //}

        public void fillGameResults()
        {
            List<GameResult> results = new List<GameResult>();
            playersToInt = new Dictionary<string, int>();
            playersFilenames = new List<string>();
            shortPlayersFilenames = new List<string>();
            foreach (var s in resultFilenames)
            {
                GameResult rez = new GameResult(s);
                results.Add(rez);
                for (int i = 0; i < 2; ++i)//находим всех игроков встречавшихся в логирующих файла
                    if (!playersToInt.ContainsKey(rez.playersFilenames[i]))
                    {
                        playersToInt.Add(rez.playersFilenames[i], playersToInt.Count);
                        playersFilenames.Add(rez.playersFilenames[i]);
                        shortPlayersFilenames.Add(rez.shortPlayersFilanames[i]);
                    }
            }

            int playersCount = playersToInt.Count;
            firstPlayerScore = new int[playersCount, playersCount];
            secondPlayerScore = new int[playersCount, playersCount];
            gamesCount = new int[playersCount, playersCount];
            foreach (var rez in results)
            {
                int f = playersToInt[rez.playersFilenames[0]];
                int s = playersToInt[rez.playersFilenames[1]];
                ++gamesCount[f, s];
                firstPlayerScore[f, s] += rez.playersScores[0];
                secondPlayerScore[f, s] += rez.playersScores[1];
            }
            //TODO continue
            firstPlayerMeanScore = new double[playersCount, playersCount];
            secondPlayerMeanScore = new double[playersCount, playersCount];

            for (int i = 0; i < playersCount; ++i)
                for (int j = 0; j < playersCount; ++j)
                {
                    firstPlayerMeanScore[i, j] = (double)firstPlayerScore[i, j] / (double)gamesCount[i, j];
                    secondPlayerMeanScore[i, j] = (double)secondPlayerScore[i, j] / (double)gamesCount[i, j];
                }




        }

        private void fillDataGrid()
        {
            int playersCount = playersToInt.Count;

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Игроки"));
            dt.Columns.Add(new DataColumn("Количество игр", typeof(int)));
            dt.Columns.Add(new DataColumn("Срендние очки первого", typeof(double)));
            dt.Columns.Add(new DataColumn("Средние очки второго", typeof(double)));
            dt.Columns.Add(new DataColumn("Сумма очков первого", typeof(int)));
            dt.Columns.Add(new DataColumn("Сумма очков втрого", typeof(int)));
            //TODO можно добавить всякие статистически параметы типо дисперисии и т.д.
            DataRow row;
            for (int i = 0; i < playersCount; ++i)
                for (int j = 0; j < playersCount; ++j)
                {
                    if (gamesCount[i, j] != 0)
                    {
                        row = dt.NewRow();
                        row[0] = shortPlayersFilenames[i] + " vs. " + shortPlayersFilenames[j];
                        row[1] = gamesCount[i, j];
                        row[2] = firstPlayerMeanScore[i, j];
                        row[3] = secondPlayerMeanScore[i, j];
                        row[4] = firstPlayerScore[i, j];
                        row[5] = secondPlayerScore[i, j];
                        dt.Rows.Add(row);
                    }
                }
            resultsDataGrid.ItemsSource = dt.DefaultView;
            foreach (var column in resultsDataGrid.Columns)
                column.IsReadOnly = true;
            resultsDataGrid.CanUserAddRows = false;
            resultsDataGrid.CanUserDeleteRows = false;
            resultsDataGrid.CanUserReorderColumns = false;
            //resultsDataGrid.CanUserSortColumns = false;
            resultsDataGrid.CanUserResizeRows = false;
        }

        private void addFilesButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> selectedFiles = showChoseFilesDialog("Text document (.txt)|*.txt");
            foreach (var item in selectedFiles)
            {
                resultFilenames.Add(item);
            }
            if (selectedFiles.Count() != 0)
            {
                fillGameResults();
                fillDataGrid();
            }

        }


        private IEnumerable<string> showChoseFilesDialog(string filter)
        {
            // Initialize the filename
            string[] fname = new string[0];

            // Configure open file dialog box
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = System.IO.Directory.GetParent(@"./").FullName;
            openFileDlg.Filter = filter; // Filter files by extension
            openFileDlg.Multiselect = true;

            // Show open file dialog box
            bool result = openFileDlg.ShowDialog() == true;

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                fname = openFileDlg.FileNames;
            }
            return fname;
        }
    }
}
