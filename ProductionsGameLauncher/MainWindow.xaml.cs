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
using Strategies;
using GUIStrategy;
using System.Security.Cryptography;

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
                        "S->Arab",
                        "A->Bernulli",
                        "B->Ceoul",
                        "C->Deli",
                        "D->Exal",
                        "E->Farid",
                        "F->Jamal",
                        "J->KKefteme|Cabaka",
                        "K->Lorka",
                        "L->Horse",
                        "H->hallo"
                    })
                });

            //TODO Дописать сюда если хотите добавить свою стратегию
            players = new List<Player>();
            players.Add(new Player("Случайная стратегия", (param) => new RandomStrategy()));
            players.Add(new Player("Умная случайная стратегия", (param) => new SmartRandomStrategy()));
            players.Add(new Player("Глупая стратегия коротких слов", (param) => new StupidShortWordsStrategy()));
            players.Add(new Player("Стратегия коротких слов", (param) => new ShortWordsStrategy()));
            

            Parameters searchParameters = new Parameters();
            searchParameters.addParameter("depth","Глубина перебора", 4);
            players.Add(new Player("Переборная стратегия",
                (param) => new SearchStrategy(param),
                searchParameters)
                );

            Parameters mixedParameters = new Parameters();
            mixedParameters.addParameter("depth", "Глубина перебора", 4);
            mixedParameters.addParameter("randomProb", "Вес случайной стратегии", 1);
            mixedParameters.addParameter("searchProb", "Вес переборной стратегии", 1);
            mixedParameters.addParameter("shortProb", "Вес стратегии коротких слов", 1);
            players.Add(new Player("Смешанная стратегия",
                (param) => new MixedStrategy(param),
                mixedParameters)
                );

            guiPlayer = new Player("Графический интерфейс", (param) => new GUIStrategyClass());

            //for (int i = 1; i <= 7; i++)
            //    depthSelectComboBox.Items.Add(i);
            //depthSelectComboBox.SelectedValue = 4;
            //depthSelectComboBox.SelectionChanged += (sender, e) => { searchPlayer.setParameter(depthSelectComboBox.SelectedValue.ToString()); };

            for (int i = 1; i <= 4; i++)
                parallelComboBox.Items.Add((i * 5));
            parallelComboBox.SelectedValue = 10;
            parallelComboBox.SelectionChanged += (sender, e) => { maxActiveThreads = (int)parallelComboBox.SelectedItem; };
            addTwoPlayers();
            //showDepthSelect();
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

            ComboBox playerComboBox = new ComboBox();
            playerComboBox.Height = 40;
            foreach (var player in avaliablePlayers)
                playerComboBox.Items.Add(player);
            playerComboBox.SelectedIndex = 0;
            playerComboBox.SelectionChanged += (sender, e) =>
            {
                int rowNum = Grid.GetRow((UIElement)sender);
                recalculatePlayers(rowNum);
                //showDepthSelect();
            };
            Grid.SetRow(playerComboBox, row);
            Grid.SetColumn(playerComboBox, 1);
            TournamentPlayersGrid.Children.Add(playerComboBox);

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
                //showDepthSelect();
            };
            Grid.SetRow(playerDeleteButton, row);
            Grid.SetColumn(playerDeleteButton, 2);
            TournamentPlayersGrid.Children.Add(playerDeleteButton);
            Button button = new Button();
            button.Content = "Настроить";
            button.Click += (sender, e) =>
            {
                Player p = null;
                int roww = Grid.GetRow(sender as Button);
                foreach (UIElement item in TournamentPlayersGrid.Children)
                    if (Grid.GetRow(item) == row && Grid.GetColumn(item) == 1)
                        p = (item as ComboBox).SelectedItem as Player;
                if (p == null) return;
                if (p.Parameters == null) return;
                ParametersWindow win = new ParametersWindow(p.Parameters);
                win.ShowDialog();
            };
            Grid.SetRow(button, row);
            Grid.SetColumn(button, 3);
            TournamentPlayersGrid.Children.Add(button);
            //showDepthSelect();

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
                //playerComboBox.SelectionChanged += (sender, e) => showDepthSelect();
                Grid.SetRow(playerComboBox, i);
                Grid.SetColumn(playerComboBox, 1);
                twoPlayersGrid.Children.Add(playerComboBox);
                //var button = getParametersButton()

                Button button = new Button();
                button.Content = "Настроить";
                button.Click += (sender, e) =>
                {
                    Player p = null;
                    int row = Grid.GetRow(sender as Button);
                    foreach (UIElement item in twoPlayersGrid.Children)
                        if (Grid.GetRow(item) == row  && Grid.GetColumn(item) == 1)
                            p = (item as ComboBox).SelectedItem as Player;
                    if (p == null) return;
                    if (p.Parameters == null)return;
                    ParametersWindow win = new ParametersWindow(p.Parameters);
                    win.ShowDialog();
                };
                Grid.SetRow(button, i);
                Grid.SetColumn(button, 2);
                twoPlayersGrid.Children.Add(button);
                //return button;
            }
        }

        //private Button getParametersButton(Parameters parameters) { 
            
        //}

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
                return;
            }
            gameSettings = new GameSettings(movesNumber, grammatic.getProductionGroups(), rs);


            if (tournamentCheckBox.IsChecked.Value)
            {
                List<Player> filenames = getTournamentPlayers();
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
                //playTournament(numberOfRounds, getTournamentPlayers());
                playTournamentLinear(numberOfRounds, getTournamentPlayers());

            }
            else
            {
                List<Player> players = getTwoPlayers();
                //List<string> parameters = getTwoPlayersParameters();
                //GameCompiler gc = new ExeSerializationGameCompiler(gameSettings, filenames, parameters);
                List<Strategy> strategies = new List<Strategy>();
                foreach (var player in players)
                    strategies.Add(player.get());
                Game gc = new Game(gameSettings, strategies);

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
        public void playTournamentLinear(int numberOfRounds, List<Player> players)
        {
            int finishedRounds = 0;
            List<string> resultFilenames = new List<string>();
            makeInactiveButtons();
            tournamentProgressBar.Visibility = Visibility.Visible;

            int playersNumber = players.Count;
            int allRounds = playersNumber * playersNumber * numberOfRounds;

            List<Strategy> strats = new List<Strategy>();
            foreach (var player in players)
            {
                strats.Add(player.get());
                strats.Last().setGameSettings(gameSettings);
            }
            

            if (!Directory.Exists(@"./logs/"))//Создаём директорию для записи туда результатов
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
                            Game game = new Game(gameSettings,
                                new Strategy[] { strats[firstIndex], strats[secondIndex] });
                            game.play();

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
        //public void playTournament(int numberOfRounds, List<Player> players)
        //{
        //    //TODO change it if CPU usage is too big.

        //    activeThreads = new List<Thread>();
        //    int finishedThreads = 0;
        //    List<string> resultFilenames = new List<string>();
        //    makeInactiveButtons();
        //    tournamentProgressBar.Visibility = Visibility.Visible;

        //    int round = 0;
        //    int firstIndex = 0, secondIndex = 0;
        //    int playersNumber = players.Count;
        //    int allRounds = playersNumber * playersNumber * numberOfRounds;

        //    if (!Directory.Exists(@"./logs/"))//Создайм директорию для записи туда результатов
        //        Directory.CreateDirectory(@"./logs/");
        //    string filename = @"./logs/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "/";
        //    if (!Directory.Exists(filename))
        //        Directory.CreateDirectory(filename);
        //    worker = new BackgroundWorker();
        //    worker.WorkerReportsProgress = true;
        //    worker.WorkerSupportsCancellation = true;
        //    worker.DoWork += (sender, e) =>
        //    {
        //        while (round < numberOfRounds || activeThreads.Count != 0)
        //        {
        //            //запускаем новые раунды, пока не достигнем макс. доступное количество потоков, или не запуустим все раунды
        //            while (activeThreads.Count < maxActiveThreads && round < numberOfRounds)
        //            {
        //                string currFilename = filename + round + "-" + firstIndex + "-" + secondIndex + ".txt";
        //                GameStart gameStart = new GameStart(gameSettings, currFilename, players[firstIndex].get(), players[secondIndex].get());
        //                resultFilenames.Add(currFilename);
        //                Thread newGCThread = new Thread(new ThreadStart(gameStart.start));
        //                newGCThread.Start();
        //                activeThreads.Add(newGCThread);
        //                secondIndex++;
        //                if (secondIndex >= playersNumber)
        //                {
        //                    firstIndex++;
        //                    secondIndex = 0;
        //                    if (firstIndex >= playersNumber)
        //                    {
        //                        round++;
        //                        firstIndex = 0;
        //                        secondIndex = 0;
        //                    }
        //                    continue;
        //                }
        //            }
        //            //освобождаем потоки/дожидаемся окончания работы всех потоков.
        //            for (int i = 0; i < activeThreads.Count;)
        //            {
        //                Thread thread = activeThreads[i];
        //                if (!thread.IsAlive)
        //                {
        //                    activeThreads.RemoveAt(i);
        //                    ++finishedThreads;
        //                    worker.ReportProgress(finishedThreads * 100 / allRounds);
        //                    continue;
        //                }
        //                ++i;
        //            }
        //            Thread.Sleep(500);
        //        }
        //    };
        //    worker.ProgressChanged += (sender, e) =>
        //    {
        //        tournamentProgressBar.Value = e.ProgressPercentage;
        //    };
        //    worker.RunWorkerCompleted += (sender, e) =>
        //    {
        //        GameResultsWindow gameResultsWidow = new GameResultsWindow(resultFilenames);
        //        this.Hide();
        //        this.Close();
        //        gameResultsWidow.ShowDialog();
        //    };
        //    worker.RunWorkerAsync();
        //}

        //private void addPlayer(Grid grid, bool forTournament) {

        //    int row = grid.RowDefinitions.Count;
        //    List<Player> avaliablePlayers = findAvaliablePlayers(row);
        //    if (avaliablePlayers.Count <= 1)
        //        addPlayerButton.IsEnabled = false;

        //    RowDefinition rowDefinition = new RowDefinition();
        //    rowDefinition.Height = new GridLength(40);
        //    grid.RowDefinitions.Add(rowDefinition);
        //    TextBlock label = new TextBlock();
        //    label.Text = "Игрок " + (row + 1);
        //    label.TextWrapping = TextWrapping.Wrap;
        //    Grid.SetRow(label, row);
        //    grid.Children.Add(label);

        //    ComboBox playerComboBox = new ComboBox();
        //    playerComboBox.Height = 40;
        //    foreach (var player in avaliablePlayers)
        //        playerComboBox.Items.Add(player);
        //    playerComboBox.SelectedIndex = 0;
        //    playerComboBox.SelectionChanged += (sender, e) =>
        //    {
        //        int rowNum = Grid.GetRow((UIElement)sender);
        //        recalculatePlayers(rowNum);
        //        //showDepthSelect();
        //    };
        //    Grid.SetRow(playerComboBox, row);
        //    Grid.SetColumn(playerComboBox, 1);
        //    grid.Children.Add(playerComboBox);

        //    Button playerDeleteButton = new Button();
        //    playerDeleteButton.Content = "Удалить";
        //    playerDeleteButton.Height = 40;
        //    playerDeleteButton.Click += (sender, e) =>
        //    {
        //        addPlayerButton.IsEnabled = true;
        //        List<UIElement> forDelete = new List<UIElement>();
        //        int rowNum = Grid.GetRow((UIElement)sender);
        //        foreach (UIElement item in grid.Children)
        //        {
        //            int t = Grid.GetRow(item);
        //            if (t == rowNum)
        //                forDelete.Add(item);
        //            else if (t > rowNum)
        //            {
        //                Grid.SetRow(item, t - 1);
        //                if (Grid.GetColumn(item) == 0)
        //                    (item as TextBlock).Text = "Игрок " + t;
        //            }
        //        }
        //        foreach (UIElement item in forDelete)
        //            grid.Children.Remove(item);
        //        grid.RowDefinitions.RemoveAt(rowNum);
        //        recalculatePlayers(rowNum - 1);
        //        //showDepthSelect();
        //    };
        //    Grid.SetRow(playerDeleteButton, row);
        //    Grid.SetColumn(playerDeleteButton, 2);
        //    grid.Children.Add(playerDeleteButton);
        //}

        private List<Player> getPlayers(Grid grid) {
            List<Player> players = new List<Player>();
            foreach (UIElement item in grid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    players.Add(cb.SelectedItem as Player);
                }
            }
            return players;
        }

        private List<Player> getTournamentPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (UIElement item in TournamentPlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    players.Add(cb.SelectedItem as Player);
                }
            }
            return players;
        }

        private List<Player> getTwoPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (UIElement item in twoPlayersGrid.Children)
            {
                if (Grid.GetColumn(item) == 1)
                {
                    ComboBox cb = item as ComboBox;
                    players.Add(cb.SelectedItem as Player);
                }
            }
            return players;
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
        Strategy player1;
        Strategy player2;

        public GameStart(GameSettings gameSettings, string logFilename, Strategy player1, Strategy player2)
        {
            this.gameSettings = gameSettings;
            this.logFilename = logFilename;
            this.player1 = player1;
            this.player2 = player2;

        }


        public void start()
        {
            Game gc = new Game(gameSettings, new Strategy[] { player1, player2 });
            gc.play();
        }
    }
}
