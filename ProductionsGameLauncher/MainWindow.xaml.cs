using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProductionsGame;
using ProductionsGameCore;
using System.IO;
using System.Threading;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameSettings gameSettings = null;
        List<Grammatic> grammatics = new List<Grammatic>(new Grammatic[] {
        new Grammatic("classic",new string[] {
                "S->ABC",
                "A->aA|B",
                "A->bc|CA",
                "A->a",
                "S->AB|AC",
                "B->b",
                "B->BBB",
                "C->AA|c",
                "C->A|B|c",
                "C->T|A",
                "B->abacabade"
            }),
        new Grammatic("long chain",new string[] {
                "S->A",
                "A->B",
                "B->C",
                "C->D",
                "D->Exal",
                "E->F",
                "F->J",
                "J->Kefteme|Cabaka",
                "K->L",
                "L->H",
                "H->hallula"
            })
        });
        private Grammatic grammatic = null;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void addPlayer()
        {
            int row = PlayersGrid.RowDefinitions.Count;
            //playersFilenames.Add("");

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(40);
            PlayersGrid.RowDefinitions.Add(rowDefinition);
            TextBlock label = new TextBlock();
            label.Text = "Игрок " + (row + 1);
            label.TextWrapping = TextWrapping.Wrap;
            Grid.SetRow(label, row);
            PlayersGrid.Children.Add(label);

            TextBlock playerFileNameTextBlock = new TextBlock();
            playerFileNameTextBlock.TextWrapping = TextWrapping.Wrap;
            Grid.SetRow(playerFileNameTextBlock, row);
            Grid.SetColumn(playerFileNameTextBlock, 1);
            PlayersGrid.Children.Add(playerFileNameTextBlock);

            Button playerFilenameChoseButton = new Button();
            playerFilenameChoseButton.Content = "Выбрать";
            playerFilenameChoseButton.Height = 40;
            playerFilenameChoseButton.Click += (sender, e) =>
            {
                string fname = showChoseFileDialog("EXE program file (.exe)|*.exe");
                if (!fname.Equals(""))
                {
                    playerFileNameTextBlock.Text = fname;
                    int rowNum = Grid.GetRow((Button)sender);
                }
            };
            Grid.SetRow(playerFilenameChoseButton, row);
            Grid.SetColumn(playerFilenameChoseButton, 2);
            PlayersGrid.Children.Add(playerFilenameChoseButton);

            Button playerDeleteButton = new Button();
            playerDeleteButton.Content = "Удалить";
            playerDeleteButton.Height = 40;
            playerDeleteButton.Click += (sender, e) =>
            {
                List<UIElement> forDelete = new List<UIElement>();
                int rowNum = Grid.GetRow((Button)sender);
                foreach (UIElement item in PlayersGrid.Children)
                {
                    int t = Grid.GetRow(item);
                    if (t == rowNum)
                        forDelete.Add(item);
                    else if (t > rowNum)
                    {
                        Grid.SetRow(item, t - 1);
                        if (Grid.GetColumn(item) == 0)
                            (item as TextBlock).Text = "Игрок " + t;
                    }
                }
                foreach (UIElement item in forDelete)
                    PlayersGrid.Children.Remove(item);
                PlayersGrid.RowDefinitions.RemoveAt(rowNum);
            };
            Grid.SetRow(playerDeleteButton, row);
            Grid.SetColumn(playerDeleteButton, 3);
            PlayersGrid.Children.Add(playerDeleteButton);
        }

        private void GrammaticChoseButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsSelect s = new SettingsSelect(grammatics);
            bool? rez = s.ShowDialog();
            if (rez == true)
            {
                grammatic = s.Result;
                GrammaticTextBlock.Text = grammatic.Name;
            }
        }

        private string showChoseFileDialog(string filter)
        {
            // Initialize the filename
            string fname = string.Empty;

            // Configure open file dialog box
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = System.IO.Directory.GetParent(@"./").FullName;
            openFileDlg.Filter = filter; // Filter files by extension

            // Show open file dialog box
            bool result = openFileDlg.ShowDialog() == true;

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                fname = openFileDlg.FileName;
            }
            return fname;
        }

        private void StartGameButton(object sender, RoutedEventArgs e)
        {
            if (gameSettings != null)
            {

                List<string> filenames = getPlayersFilenames();
                foreach (string filename in filenames)
                {
                    if (!File.Exists(filename))
                    {
                        MessageBox.Show("Игрок не выбран или указанного файла не существует.");
                        return;
                    }
                }
                int numberOfRounds = 1;
                if (!int.TryParse(roundsNumberTextBox.Text, out numberOfRounds))
                {
                    MessageBox.Show("Указано некорректное количество раундов.");
                    return;
                }
                if (filenames.Count < gameSettings.NumberOfPlayers)
                {
                    MessageBox.Show("Указано недостаточное количество игроков, необходимо минимум " + gameSettings.NumberOfPlayers + " игроков.");
                    return;
                }

                List<string> rezultsFileames = playTournament(numberOfRounds, tournamentCheckBox.IsChecked.Value, getPlayersFilenames());
                GameResults gameResultsWidow = new GameResults(rezultsFileames);
                this.Hide();
                gameResultsWidow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Сначала задайте параметры игры и выберите игроков.");
            }
        }

        public List<string> playTournament(int numberOfRounds, bool isTournament, List<string> playersFilenames)
        {
            //TODO change it if CPU usage is too big.
            const int maxActiveThreads = 40;
            List<Thread> activeThreads = new List<Thread>();
            int finishedThreads = 0;
            List<string> resultFilenames = new List<string>();


            int round = 0;
            int firstIndex = 0, secondIndex = 0;
            int playersNumber = playersFilenames.Count;
            int allRounds = playersNumber * playersNumber * numberOfRounds;
            if (!isTournament)
                secondIndex = 1;

            if (!Directory.Exists(@"./logs/"))//Создайм директорию для записи туда результатов
                Directory.CreateDirectory(@"./logs/");
            string filename = @"./logs/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "/";
            if (!Directory.Exists(filename))
                Directory.CreateDirectory(filename);

            while (round < numberOfRounds || activeThreads.Count != 0)
            {
                //запускаем новые раунды, пока не достигнем макс. доступное количество потоков, или не запуустим все раунды
                while (activeThreads.Count < maxActiveThreads && round < numberOfRounds)
                {
                    ExeSerializationGameCompiler gc =
                        new ExeSerializationGameCompiler(
                            gameSettings,
                            new string[] { playersFilenames[firstIndex], playersFilenames[secondIndex] },
                            filename + round + "-" + firstIndex + "-" + secondIndex + ".txt"
                        );
                    resultFilenames.Add(gc.LogFilename);
                    Thread newGCThread = new Thread(new ThreadStart(gc.play));
                    newGCThread.Start();
                    activeThreads.Add(newGCThread);
                    if (isTournament)
                    {
                        secondIndex++;
                        if (secondIndex >= playersNumber)
                        {
                            firstIndex++;
                            secondIndex = 0;
                            if (firstIndex >= playersNumber)
                            {
                                round++;
                                firstIndex = 0;
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
                        continue;
                    }
                    ++i;
                }
            }
            return resultFilenames;
        }

        private List<string> getPlayersFilenames()
        {
            List<string> filenames = new List<string>();
            foreach (UIElement item in PlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    filenames.Add((item as TextBlock).Text);
                }
            }
            return filenames;
        }

        private void roundsNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string s = e.Text;
            foreach (char c in s)
                if (!(c >= '0' && c <= '9'))
                    e.Handled = true;
        }

        private void addPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            addPlayer();
        }

        private void resultButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            GameResults rez = new GameResults();
            rez.Show();
            this.Close();
        }
    }
}
