﻿using Microsoft.Win32;
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
using System.IO;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Interaction logic for GameResults.xaml
    /// </summary>
    public partial class GameResultsWindow : Window
    {
        //файлы стратегий игроков
        List<string> playersFilenames;
        List<string> shortPlayersFilenames;

        //файлы результатов раундов игркоков
        //List<string> resultFilenames = new List<string>();

        ObservableCollection<GameResult> results;

        Dictionary<string, int> playersToInt;



        //в firstPlayerScores[i,j] - сумма очков игрока i, в игре где игрок i - ходит первым, а j - вторым
        //в secondPlayerScores[i,j] - сумма очков игрока j, в игре где игрок i - ходит первым, а j - вторым
        //в gamesCount[i,j] - кличество игр, где игрок i - ходит первым, а j - вторым
        int[,] firstPlayerScore;
        int[,] secondPlayerScore;
        int[,] gamesCount;

        int[,] firstPlayerWin;
        int[,] secondPlayerWin;

        //в firstPlayerMeanScores[i,j] - среднее арифметическое очков игрока i, в игре где игрок i - ходит первым, а j - вторым
        //в secondPlayerMeanScores[i,j] - среднее арифметическое очков игрока j, в игре где игрок i - ходит первым, а j - вторым
        double[,] firstPlayerMeanScore;
        double[,] secondPlayerMeanScore;

        //TODo hide button!!!!

        public GameResultsWindow()
        {
            InitializeComponent();
            results = new ObservableCollection<GameResult>();
            SelectedFilesListBox.SelectionMode = SelectionMode.Single;
            SelectedFilesListBox.ItemsSource = results;
        }

        public GameResultsWindow(List<string> resultFilenames)
        {
            InitializeComponent();
            this.results = new ObservableCollection<GameResult>();
            addResults(resultFilenames);
            fillGameResults();
            fillDataGrid();
            SelectedFilesListBox.SelectionMode = SelectionMode.Single;
            SelectedFilesListBox.ItemsSource = this.results;
        }

        private void addResults(IEnumerable<string> filenames)
        {
            foreach (var fname in filenames)
                addResults(fname);
        }

        private void addResults(string filename)
        {
            results.Add(new GameResult(filename));
        }

        public void fillGameResults()
        {
            List<GameResult> results = new List<GameResult>();
            playersToInt = new Dictionary<string, int>();
            playersFilenames = new List<string>();
            shortPlayersFilenames = new List<string>();
            foreach (var s in this.results)
            {
                GameResult rez = s;
                if (rez.playersScores.Count != 2 || rez.playersFilenames.Count != 2)
                    continue;
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
            firstPlayerWin = new int[playersCount, playersCount];
            secondPlayerWin = new int[playersCount, playersCount];
            gamesCount = new int[playersCount, playersCount];
            foreach (var rez in results)
            {
                int f = playersToInt[rez.playersFilenames[0]];
                int s = playersToInt[rez.playersFilenames[1]];
                ++gamesCount[f, s];
                firstPlayerScore[f, s] += rez.playersScores[0];
                secondPlayerScore[f, s] += rez.playersScores[1];
                if(rez.winner==0)
                    firstPlayerWin[f, s]++;
                else if (rez.winner==1)
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

        private void fillDataGrid()
        {
            int playersCount = playersToInt.Count;

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Игроки"));
            dt.Columns.Add(new DataColumn("К-во игр", typeof(int)));
            dt.Columns.Add(new DataColumn("Средние очки первого", typeof(double)));
            dt.Columns.Add(new DataColumn("Средние очки второго", typeof(double)));
            dt.Columns.Add(new DataColumn("Побед первого", typeof(double)));
            dt.Columns.Add(new DataColumn("Побед второго", typeof(double)));
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
                        row[4] = firstPlayerWin[i, j];
                        row[5] = secondPlayerWin[i, j];
                        row[6] = firstPlayerScore[i, j];
                        row[7] = secondPlayerScore[i, j];
                        dt.Rows.Add(row);
                    }
                }
            resultsDataGrid.ItemsSource = dt.DefaultView;
            foreach (var column in resultsDataGrid.Columns)
                column.IsReadOnly = true;
            resultsDataGrid.CanUserAddRows = false;
            resultsDataGrid.CanUserDeleteRows = false;
            resultsDataGrid.CanUserReorderColumns = false;
            resultsDataGrid.CanUserResizeRows = false;
        }

        private void writeToFile() {
            StreamWriter sf = new StreamWriter("./out.txt");
            int[] trueIndexes = new int[4];
            trueIndexes[0] = find(" ./RandomStrategy.exe");
            trueIndexes[1] = find(" ./StupidShortWordsStrategy.exe");
            trueIndexes[2] = find(" ./ShortWordsStrategy.exe");
            trueIndexes[3] = find(" ./SearchStrategy.exe");
            for (int i = 0; i < 4; i++)
            {
                
                for (int j = 0; j < 4; j++)
                {
                    sf.Write(" & ");
                    sf.Write((int)firstPlayerMeanScore[trueIndexes[i], trueIndexes[j]]);
                    sf.Write("/");
                    sf.Write((int)secondPlayerMeanScore[trueIndexes[i], trueIndexes[j]]);
                }
                sf.WriteLine("\\\\");
                sf.WriteLine("\\hline");
            }

            sf.WriteLine("\\hline"); sf.WriteLine("\\hline"); sf.WriteLine("\\hline");
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sf.Write(" & ");
                    sf.Write(firstPlayerWin[trueIndexes[i], trueIndexes[j]]);
                    sf.Write("/");
                    sf.Write(secondPlayerWin[trueIndexes[i], trueIndexes[j]]);
                }
                sf.WriteLine("\\\\");
                sf.WriteLine("\\hline");
            }
            sf.Close();
        }

        private int find(string s) {
            foreach (var v in playersToInt)
                if (v.Key == s) return v.Value;
            return -1;
        }

        private void addFilesButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> selectedFiles = showChoseFilesDialog("Text document (.txt)|*.txt");
            foreach (var item in selectedFiles)
                addResults(item);
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

        private void sookSelectedGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFilesListBox.SelectedItem == null) {
                MessageBox.Show("Не один файл не выбран!");
                return;
            }
            GameResult rez = SelectedFilesListBox.SelectedItem as GameResult;
            LookGame lg = new LookGame(rez.filename);
            lg.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            writeToFile();
        }
    }
}
