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

namespace ProductsGameLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameSettings gameSettings;
        private string[] playersFilenames;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void readConfig()
        {
            string filename = SettingsFileChoseTextBlock.Text;
            //TODO catch exception
            gameSettings = GameSettings.ReadFromFile(filename);
            fillPlayersGrid();
        }

        private void fillPlayersGrid() { 
            PlayersGrid.Children.Clear();
            PlayersGrid.RowDefinitions.Clear();
            if (gameSettings == null) return;
            playersFilenames = new string[gameSettings.NumberOfPlayers];
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
                this.Hide();
                GameResults gameResultsWidow = new GameResults(new GameCompiler(gameSettings, playersFilenames.AsEnumerable()));
                gameResultsWidow.Show();
                this.Close();
            }
            else {
                MessageBox.Show("Сначала задайте параметры игры и выберите игроков.");
            }
        }
    }
}
