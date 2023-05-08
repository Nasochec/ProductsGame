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
using ProductsGame;
using ProductionsGameCore;
using System.IO;

namespace ProductsGameLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameSettings gameSettings = null;
        private List<string> playersFilenames;

        public MainWindow()
        {
            InitializeComponent();
            playersFilenames = new List<string>();
        }

        private void readConfig()
        {
            string filename = SettingsFileChoseTextBlock.Text;
            try
            {
                gameSettings = GameSettings.ReadFromFile(filename);
                if (gameSettings.NumberOfPlayers != 2)
                {
                    MessageBox.Show("Введена неверная конфигурация, количество игроков должно быть равно 2.");
                    gameSettings = null;
                }
                else if (gameSettings.IsBankShare == false)
                {
                    MessageBox.Show("Введена неверная конфигурация, банк продукций должен быть общим.");
                    gameSettings = null;
                }
                else if (gameSettings.ProductionsCount != 11)
                {
                    MessageBox.Show("Введена неверная конфигурация, количество продукций должно быть равно 11.");
                    gameSettings = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка считывания конфигурации. Конфигурация была в неверном формате.");
            }
        }


        private void fillPlayersGrid()
        {
            PlayersGrid.Children.Clear();
            PlayersGrid.RowDefinitions.Clear();
            if (gameSettings == null) return;

            for (int i = 0; i < gameSettings.NumberOfPlayers; ++i)
            {
                PlayersGrid.RowDefinitions.Add(new RowDefinition());
                TextBlock label = new TextBlock();
                label.Text = "Игрок " + (i + 1);
                label.TextWrapping = TextWrapping.Wrap;
                Grid.SetRow(label, i);
                PlayersGrid.Children.Add(label);

                TextBlock playerFileNameTextBlock = new TextBlock();
                playerFileNameTextBlock.TextWrapping = TextWrapping.Wrap;
                Grid.SetRow(playerFileNameTextBlock, i);
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
                        playersFilenames[rowNum] = fname;
                    }
                };
                Grid.SetRow(playerFilenameChoseButton, i);
                Grid.SetColumn(playerFilenameChoseButton, 2);
                PlayersGrid.Children.Add(playerFilenameChoseButton);

            }
        }

        private void addPlayer()
        {
            int row = PlayersGrid.RowDefinitions.Count;
            playersFilenames.Add("");

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

        private void SettigsFileChoseButton_Click(object sender, RoutedEventArgs e)
        {
            string fname = showChoseFileDialog("XML documents (.xml)|*.xml");

            if (!fname.Equals(""))
            {
                SettingsFileChoseTextBlock.Text = fname;
                readConfig();
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

        private void SettingsMoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsChageWindow settingsChageWindow = new SettingsChageWindow(gameSettings);
            settingsChageWindow.Show();
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
                this.Hide();
               
                GameResults gameResultsWidow = new GameResults(gameSettings, filenames, numberOfRounds);
                gameResultsWidow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Сначала задайте параметры игры и выберите игроков.");
            }
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
    }
}
