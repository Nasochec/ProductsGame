using Microsoft.Win32;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Interaction logic for GameResults.xaml
    /// </summary>
    public partial class GameResultsWindow : Window
    {
        GameResults result = new GameResults();

        public GameResultsWindow()
        {
            InitializeComponent();
            SelectedFilesListBox.SelectionMode = SelectionMode.Single;
            SelectedFilesListBox.ItemsSource = result.results;
        }

        public GameResultsWindow(List<string> resultFilenames)
        {
            InitializeComponent();
            result.addResults(resultFilenames);
            result.fillGameResults();
            fillDataGrid();
            SelectedFilesListBox.SelectionMode = SelectionMode.Single;
            SelectedFilesListBox.ItemsSource = result.results;
        }

        private void fillDataGrid()
        {
            int playersCount = result.playersToInt.Count;

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Первый игрок"));
            dt.Columns.Add(new DataColumn("Второй Игрок"));
            dt.Columns.Add(new DataColumn("К-во игр", typeof(int)));
            dt.Columns.Add(new DataColumn("Побед первого", typeof(double)));
            dt.Columns.Add(new DataColumn("Побед второго", typeof(double)));
            dt.Columns.Add(new DataColumn("Ничьи", typeof(double)));
            dt.Columns.Add(new DataColumn("Средние очки первого", typeof(double)));
            dt.Columns.Add(new DataColumn("Средние очки второго", typeof(double)));
            dt.Columns.Add(new DataColumn("Сумма очков первого", typeof(int)));
            dt.Columns.Add(new DataColumn("Сумма очков втрого", typeof(int)));
            //TODO можно добавить всякие статистически параметы типо дисперисии и т.д.
            DataRow row;
            int colIndex = 0;
            for (int i = 0; i < playersCount; ++i)
                for (int j = 0; j < playersCount; ++j)
                {
                    if (result.gamesCount[i, j] != 0)
                    {
                        colIndex = 0;
                        row = dt.NewRow();
                        row[colIndex++] = result.playersNames[i];
                        row[colIndex++] = result.playersNames[j];
                        row[colIndex++] = result.gamesCount[i, j];
                        row[colIndex++] = result.firstPlayerWin[i, j];
                        row[colIndex++] = result.secondPlayerWin[i, j];
                        row[colIndex++] = result.draw[i, j];
                        row[colIndex++] = result.firstPlayerMeanScore[i, j];
                        row[colIndex++] = result.secondPlayerMeanScore[i, j];
                        row[colIndex++] = result.firstPlayerScore[i, j];
                        row[colIndex++] = result.secondPlayerScore[i, j];
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

        private void addFilesButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> selectedFiles = showChoseFilesDialog("Text document (.txt)|*.txt");
            foreach (var item in selectedFiles)
                result.addResults(item);
            if (selectedFiles.Count() != 0)
            {
                result.fillGameResults();
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

        private void lookSelectedGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFilesListBox.SelectedItem == null)
            {
                MessageBox.Show("Не один файл не выбран!");
                return;
            }
            FileGameResult rez = SelectedFilesListBox.SelectedItem as FileGameResult;
            LookGame lg = new LookGame(rez.filename);
            lg.ShowDialog();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            result.writeToFile(".out.txt");
        }
    }
}
