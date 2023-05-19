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
using System.Net;
using System.ComponentModel;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameSettings gameSettings = null;
        //конфигурация генератора случайных чисел соответствующая броску двух d6 кубиков
        RandomSettings rs;
        //Предложенные грамматики 
        List<Grammatic> grammatics;
        //Написанные стратегии игроков.
        //TODO Дописать сюда если хотите добавить свою стратегию
        Player searchPlayer;
        List<Player> players;

        Player guiPlayer;

        BackgroundWorker worker;

        private Grammatic grammatic = null;


        public MainWindow()
        {
            InitializeComponent();
            rs = new RandomSettings(36, new int[] { 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1 });
            grammatics = new List<Grammatic>(new Grammatic[] {
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
                        "J->KKefteme|Cabaka",
                        "K->L",
                        "L->H",
                        "H->hallula"
                    })
                });
            searchPlayer = new Player("Переборная стратегия.", @"./SearchStrategy.exe");
            players = new List<Player>(new Player[] {
                    new Player("Случайная стратегия.",@"./RandomStrategy.exe"),
                    new Player("Стратегия коротких слов.",@"./ShortWordsStrategy.exe"),
                    new Player("Глупая стратегия коротких слов.",@"./StupidShortWordsStrategy.exe"),
                    searchPlayer
                });
            guiPlayer = new Player("Графический интерфейс", @"./GUIStrategy.exe");
            for (int i = 1; i <= 8; i++)
                depthSelectComboBox.Items.Add(i);
            depthSelectComboBox.SelectedValue = 4;
            depthSelectComboBox.SelectionChanged += (sender, e) => { searchPlayer.setParameter(depthSelectComboBox.SelectedValue.ToString()); };

            for (int i = 1; i <= 4; i++)
                parallelComboBox.Items.Add((i*5));
            parallelComboBox.SelectedValue = 10;
            parallelComboBox.SelectionChanged += (sender, e) => { maxActiveThreads = (int)parallelComboBox.SelectedItem; };
            addTwoPlayers();
            showDepthSelect();
        }

        private List<Player> findAvaliablePlayers(int row)
        {
            List<Player> avaliablePlayers = new List<Player>();
            if (row == 0)
            {
                foreach (var player in players)
                    avaliablePlayers.Add(player);
            }
            else
            {
                ComboBox prev = null;
                foreach (UIElement item in TournamentPlayersGrid.Children)
                    if (Grid.GetRow(item) == row - 1 && Grid.GetColumn(item) == 1)
                        prev = item as ComboBox;
                foreach (var player in prev.Items)
                {
                    avaliablePlayers.Add(player as Player);
                }
                avaliablePlayers.Remove(prev.SelectedItem as Player);
            }
            return avaliablePlayers;
        }

        private void recalculatePlayers(int rowNum)
        {
            foreach (UIElement item in TournamentPlayersGrid.Children)
            {
                int t = Grid.GetRow(item);
                if (t > rowNum && Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    List<Player> avaliable = findAvaliablePlayers(t);
                    Player selected = cb.SelectedItem as Player;
                    cb.Items.Clear();
                    foreach (var player in avaliable)
                        cb.Items.Add(player);
                    if (avaliable.IndexOf(selected) != -1)
                        cb.SelectedItem = selected;
                    else
                        cb.SelectedIndex = 0;
                }
            }
        }

        private void addPlayer()
        {
            int row = TournamentPlayersGrid.RowDefinitions.Count;
            List<Player> avaliablePlayers = findAvaliablePlayers(row);
            if (avaliablePlayers.Count <= 1)
                addPlayerButton.IsEnabled = false;

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(40);
            TournamentPlayersGrid.RowDefinitions.Add(rowDefinition);
            TextBlock label = new TextBlock();
            label.Text = "Игрок " + (row + 1);
            label.TextWrapping = TextWrapping.Wrap;
            Grid.SetRow(label, row);
            TournamentPlayersGrid.Children.Add(label);

            ComboBox playerFilenameChoseButton = new ComboBox();
            playerFilenameChoseButton.Height = 40;
            foreach (var player in avaliablePlayers)
                playerFilenameChoseButton.Items.Add(player);
            playerFilenameChoseButton.SelectedIndex = 0;
            playerFilenameChoseButton.SelectionChanged += (sender, e) =>
            {
                int rowNum = Grid.GetRow((UIElement)sender);
                recalculatePlayers(rowNum);
                showDepthSelect();
            };
            Grid.SetRow(playerFilenameChoseButton, row);
            Grid.SetColumn(playerFilenameChoseButton, 1);
            TournamentPlayersGrid.Children.Add(playerFilenameChoseButton);

            Button playerDeleteButton = new Button();
            playerDeleteButton.Content = "Удалить";
            playerDeleteButton.Height = 40;
            playerDeleteButton.Click += (sender, e) =>
            {
                addPlayerButton.IsEnabled = true;
                List<UIElement> forDelete = new List<UIElement>();
                int rowNum = Grid.GetRow((UIElement)sender);
                foreach (UIElement item in TournamentPlayersGrid.Children)
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
                    TournamentPlayersGrid.Children.Remove(item);
                TournamentPlayersGrid.RowDefinitions.RemoveAt(rowNum);
                recalculatePlayers(rowNum - 1);
                showDepthSelect();
            };
            Grid.SetRow(playerDeleteButton, row);
            Grid.SetColumn(playerDeleteButton, 2);
            TournamentPlayersGrid.Children.Add(playerDeleteButton);
            showDepthSelect();

        }

        //Отображает на экране возможность выбора глубины перебора переборной стратегии.
        private void showDepthSelect()
        {
            //если турнир
            bool found = false;
            if (tournamentCheckBox.IsChecked.Value)
            {
                foreach (UIElement item in TournamentPlayersGrid.Children)
                {
                    if (Grid.GetColumn(item) == 1)
                    {
                        ComboBox cb = item as ComboBox;
                        if ((cb.SelectedItem as Player) == searchPlayer)
                            found = true;
                    }
                }
            }
            else
            {
                foreach (UIElement item in twoPlayersGrid.Children)
                {
                    if (Grid.GetColumn(item) == 1)
                    {
                        ComboBox cb = item as ComboBox;
                        if ((cb.SelectedItem as Player) == searchPlayer)
                            found = true;
                    }
                }
            }
            if (found)
            {
                depthSelectDockPanel.Visibility = Visibility.Visible;
            }
            else
            {
                depthSelectDockPanel.Visibility = Visibility.Hidden;
            }
        }

        private void addTwoPlayers()
        {
            for (int i = 0; i < 2; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(40);
                twoPlayersGrid.RowDefinitions.Add(rowDefinition);
                TextBlock label = new TextBlock();
                label.Text = "Игрок " + (i + 1);
                label.TextWrapping = TextWrapping.Wrap;
                Grid.SetRow(label, i);
                twoPlayersGrid.Children.Add(label);
                ComboBox playerComboBox = new ComboBox();
                playerComboBox.VerticalAlignment = VerticalAlignment.Center;
                playerComboBox.Height = 20;
                foreach (var item in players)
                    playerComboBox.Items.Add(item);
                playerComboBox.Items.Add(guiPlayer);
                playerComboBox.SelectedIndex = 0;
                playerComboBox.SelectionChanged += (sender, e) => showDepthSelect();
                Grid.SetRow(playerComboBox, i);
                Grid.SetColumn(playerComboBox, 1);
                twoPlayersGrid.Children.Add(playerComboBox);
            }
        }

        //TODO 
        //добавить просмотр раундов из окна результатов раунда
        //скрыть от пользователя exe, подменитиь выбором из списка+
        //
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

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            int movesNumber;
            if (!int.TryParse(movesNumberTextBox.Text, out movesNumber) || movesNumber <= 0)
            {
                MessageBox.Show("Указано неверное количество ходов.");
            }
            gameSettings = new GameSettings(true, 2, movesNumber, grammatic.getProductionGroups(), rs);


            if (tournamentCheckBox.IsChecked.Value)
            {
                List<string> filenames = getTournamentPlayersFilenames();
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
                if (filenames.Count < 1)
                {
                    MessageBox.Show("Указано недостаточное количество игроков, необходим 1 игрок для запуска турнира.");
                    return;
                }
                playTournament(numberOfRounds, getTournamentPlayersFilenames(), getTournamentPlayersParameters());
                //playTournamentLinear(numberOfRounds, getTournamentPlayersFilenames());

            }
            else
            {
                List<string> filenames = getTwoPlayersFilenames();
                List<string> parameters = getTwoPlayersParameters();
                GameCompiler gc = new ExeSerializationGameCompiler(gameSettings, filenames, parameters);
                LookGame look = new LookGame(gc);
                this.Hide();
                look.ShowDialog();
                this.Close();
            }
        }

        private void makeInactiveButtons()
        {
            movesNumberTextBox.IsEnabled = false;
            roundsNumberTextBox.IsEnabled = false;
            tournamentCheckBox.IsEnabled = false;
            GrammaticChoseButton.IsEnabled = false;
            helpButton.IsEnabled = false;
            startGameButton.IsEnabled = false;
            resultButton.IsEnabled = false;
            depthSelectComboBox.IsEnabled = false;
        }

        //Вариант проигрываения партий турнира по-очереди
        public void playTournamentLinear(int numberOfRounds, List<string> playersFilenames)
        {
            int finishedRounds = 0;
            List<string> resultFilenames = new List<string>();
            makeInactiveButtons();
            tournamentProgressBar.Visibility = Visibility.Visible;

            int playersNumber = playersFilenames.Count;
            int allRounds = playersNumber * playersNumber * numberOfRounds;

            if (!Directory.Exists(@"./logs/"))//Создайм директорию для записи туда результатов
                Directory.CreateDirectory(@"./logs/");
            string filename = @"./logs/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "/";
            if (!Directory.Exists(filename))
                Directory.CreateDirectory(filename);
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (sender, e) =>
            {
                //запускаем новые раунды, пока не достигнем макс. доступное количество потоков, или не запуустим все раунды
                for (int round = 0; round < numberOfRounds; ++round)
                    for (int firstIndex = 0; firstIndex < playersNumber; ++firstIndex)
                        for (int secondIndex = 0; secondIndex < playersNumber; ++secondIndex)
                        {
                            ExeSerializationGameCompiler gc =
                                new ExeSerializationGameCompiler(
                                    gameSettings,
                                    new string[] { playersFilenames[firstIndex], playersFilenames[secondIndex] },
                                    null,//TODO implemetd paremeters
                                    filename + round + "-" + firstIndex + "-" + secondIndex + ".txt"
                                );
                            resultFilenames.Add(gc.LogFilename);
                            gc.play();
                            ++finishedRounds;
                            worker.ReportProgress(finishedRounds * 100 / allRounds);
                        }

            };
            worker.ProgressChanged += (sender, e) =>
            {
                tournamentProgressBar.Value = e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                GameResultsWindow gameResultsWidow = new GameResultsWindow(resultFilenames);
                this.Hide();
                gameResultsWidow.Show();
                this.Close();
            };
            worker.RunWorkerAsync();
            //return resultFilenames;
        }

        List<Thread> activeThreads;
        int maxActiveThreads = 10;
        //Вариант проигрывания игр турнира параллельно, работает в несколько раз быстрее чем линейный вариант, но больше затраты времени
        public void playTournament(int numberOfRounds, List<string> playersFilenames, List<string> parameters)
        {
            //TODO change it if CPU usage is too big.
            
            activeThreads = new List<Thread>();
            int finishedThreads = 0;
            List<string> resultFilenames = new List<string>();
            makeInactiveButtons();
            tournamentProgressBar.Visibility = Visibility.Visible;

            int round = 0;
            int firstIndex = 0, secondIndex = 0;
            int playersNumber = playersFilenames.Count;
            int allRounds = playersNumber * playersNumber * numberOfRounds;

            if (!Directory.Exists(@"./logs/"))//Создайм директорию для записи туда результатов
                Directory.CreateDirectory(@"./logs/");
            string filename = @"./logs/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "/";
            if (!Directory.Exists(filename))
                Directory.CreateDirectory(filename);
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (sender, e) =>
            {
                while (round < numberOfRounds || activeThreads.Count != 0)
                {
                    //запускаем новые раунды, пока не достигнем макс. доступное количество потоков, или не запуустим все раунды
                    while (activeThreads.Count < maxActiveThreads && round < numberOfRounds)
                    {
                        string currFilename = filename + round + "-" + firstIndex + "-" + secondIndex + ".txt";
                        GameStart gameStart = new GameStart(gameSettings, currFilename, playersFilenames[firstIndex], playersFilenames[secondIndex], parameters[firstIndex], parameters[secondIndex]);
                        resultFilenames.Add(currFilename);
                        Thread newGCThread = new Thread(new ThreadStart(gameStart.start));

                        newGCThread.Start();
                        activeThreads.Add(newGCThread);
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
                    //освобождаем потоки/дожидаемся окончания работы всех потоков.
                    for (int i = 0; i < activeThreads.Count;)
                    {
                        Thread thread = activeThreads[i];
                        if (!thread.IsAlive)
                        {
                            activeThreads.RemoveAt(i);
                            ++finishedThreads;
                            worker.ReportProgress(finishedThreads * 100 / allRounds);
                            continue;
                        }
                        ++i;
                    }
                    Thread.Sleep(500);
                }
            };
            worker.ProgressChanged += (sender, e) =>
            {
                tournamentProgressBar.Value = e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                GameResultsWindow gameResultsWidow = new GameResultsWindow(resultFilenames);
                this.Hide();
                this.Close();
                gameResultsWidow.ShowDialog();
            };
            worker.RunWorkerAsync();
        }

        private List<string> getTournamentPlayersFilenames()
        {
            List<string> filenames = new List<string>();
            foreach (UIElement item in TournamentPlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    filenames.Add((cb.SelectedItem as Player).Filename);
                }
            }
            return filenames;
        }

        private List<string> getTournamentPlayersParameters()
        {
            List<string> parameters = new List<string>();
            foreach (UIElement item in TournamentPlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    parameters.Add((cb.SelectedItem as Player).Parameter);
                }
            }
            return parameters;
        }

        private List<string> getTwoPlayersFilenames()
        {
            List<string> filenames = new List<string>();
            foreach (UIElement item in twoPlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    filenames.Add((cb.SelectedItem as Player).Filename);
                }
            }
            return filenames;
        }

        private List<string> getTwoPlayersParameters()
        {
            List<string> parameters = new List<string>();
            foreach (UIElement item in twoPlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    parameters.Add((cb.SelectedItem as Player).Parameter);
                }
            }
            return parameters;
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
            GameResultsWindow rez = new GameResultsWindow();
            this.Close();
            rez.ShowDialog();
        }

        private void movesNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string s = e.Text;
            foreach (char c in s)
                if (!(c >= '0' && c <= '9'))
                    e.Handled = true;
        }

        private void tournamentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            tournamentViewGrid.Visibility = Visibility.Visible;
            viewGrid.Visibility = Visibility.Hidden;
        }

        private void tournamentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            tournamentViewGrid.Visibility = Visibility.Hidden;
            viewGrid.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (worker != null)
                if (worker.IsBusy)
                {
                    worker.CancelAsync();
                    if (activeThreads.Count > 0)
                        foreach (var thread in activeThreads)
                        {
                            if (thread.IsAlive)
                                thread.Abort();
                        }
                }
        }
    }


    internal class GameStart
    {
        GameSettings gameSettings;
        string logFilename;
        string player1Filename;
        string player2Filename;
        string player1Parameter;
        string player2Parameter;

        public GameStart(GameSettings gameSettings, string logFilename, string player1Filename, string player2Filename, string player1Parameter, string player2Parameter)
        {
            this.gameSettings = gameSettings;
            this.logFilename = logFilename;
            this.player1Filename = player1Filename;
            this.player2Filename = player2Filename;
            this.player1Parameter = player1Parameter;
            this.player2Parameter = player2Parameter;
        }


        public void start()
        {
            ExeSerializationGameCompiler gc =
                new ExeSerializationGameCompiler(
                    gameSettings,
                    new string[] { player1Filename, player2Filename },
                    new string[] { player1Parameter, player2Parameter },
                    logFilename
                );
            gc.play();
        }
    }
}
