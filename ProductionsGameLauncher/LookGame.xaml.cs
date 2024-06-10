using ProductionsGame;
using ProductionsGameCore;
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
using System.Windows.Shapes;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Interaction logic for LookGame.xaml
    /// </summary>
    public partial class LookGame : Window
    {
        GameHistory gameHistory;
        ListBox[] playersListBoxes = new ListBox[2];
        TextBlock[] playersScores = new TextBlock[2];


        public LookGame(Game gc)
        {
            InitializeComponent();
            try
            {
                gameHistory = new CompilerGameHistory(gc);
            }
            catch(Exception ex) {
                MessageBox.Show(string.Format("Возникла ошибка при попытке просмотра игры.", ex.Message));
                this.Close();
            }
            firstInit();
        }

        public LookGame(string filename)
        {
            InitializeComponent();
            try
            {
                gameHistory = new FileGameHistory(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Возникла ошибка при попытке прочитать запись игры.", ex.Message));
                this.Close();
            }
            firstInit();
        }

        private void firstInit()
        {
            totalMovesTextBlock.Text = "Всего ходов:" + gameHistory.GameSettings.NumberOfMoves;
            firstPlayerNameTextBlock.Text = String.Format("Выводы первого игрока ({0})", gameHistory.playersNames[0]);
            secondPlayerNameTextBlock.Text = String.Format("Выводы второго игрока ({0})", gameHistory.playersNames[1]);
            playersListBoxes[0] = firstPlayerWordsListBox;
            playersListBoxes[1] = secondplayerWordsListBox;
            playersScores[0] = firstPlayerScoreTextBlock;
            playersScores[1] = secondPlayerScoreTextBlock;
            fillProductionsTextBlock();
            fillPlayersInfo();
            fillAnotherInfo();
        }

        private void fillProductionsTextBlock()
        {
            StringBuilder sb = new StringBuilder();
            var prod = gameHistory.GameSettings.GetProductions().GetEnumerator();
            var count = gameHistory.getCurrentBank().GetEnumerator();
            int i = 1;
            while (prod.MoveNext() && count.MoveNext())
            {
                sb.AppendLine((i++) + ". " + count.Current + " " + prod.Current.ToString());
            }
            productionsTextBlock.Text = sb.ToString();
        }

        private void fillPlayersInfo()
        {
            currentMoveTextBlock.Text = "Ход " + (gameHistory.currentMoveNumber + 1);
            currentPlayerTextBlock.Text = "Сходил игрок: " + (gameHistory.currentPlayer + 1);
            productionTextBlock.Text = "Выпала продукция " + (gameHistory.currentProductionGroup + 1);

            for (int player = 0; player < 2; ++player)
            {
                playersListBoxes[player].Items.Clear();
                foreach (var word in gameHistory.getPlayerWords(player))
                {
                    playersListBoxes[player].Items.Add(word);
                }
                playersScores[player].Text = "Очки " + (player + 1) + " игрока: " + countScore(gameHistory.getPlayerWords(player));
            }
            currentMoveListBox.Items.Clear();
            List<string> moves = new List<string>();
            if (gameHistory.currentPlayer != -1)
            {
                var words = gameHistory.getPlayerWords(gameHistory.currentPlayer).Count();
                for (int i = 0; i < words; i++)
                    moves.Add("");
                foreach (var move in gameHistory.getStrategicMove())
                {
                    int windex = move.WordNumber;
                    if (windex == -1)
                        windex = moves.Count - 1;
                    if (moves[windex] == "")
                    {
                        moves[windex] = move.PrevWord;
                    }
                    moves[windex] += "->" + move.NewWord;
                }
                foreach (var move in moves)
                {
                    if (move.Length != 0)
                    {
                        currentMoveListBox.Items.Add(move);
                    }
                }
            }

        }

        private void fillAnotherInfo() {
            if (!gameHistory.hasNextMove())
            {
                forwardMoveButton.IsEnabled = false;
                if (gameHistory.IsGameFailed)
                    messageTextBlock.Text = String.Format("Игра окончена с ошибкой из-за {0} игрока.", gameHistory.failedPlayer);
                else
                    messageTextBlock.Text = String.Format("Игра успешно окончена.");
            }
            else
            {
                forwardMoveButton.IsEnabled = true;
                messageTextBlock.Text = "";
            }

            if (!gameHistory.hasPrevMove()) backMoveButton.IsEnabled = false;
            else backMoveButton.IsEnabled = true;
            
        }

        public int countScore(IEnumerable<string> words)
        {
            int sum = 0;
            foreach (var word in words)
            {
                bool found = false;
                foreach (var c in word)
                    if (c >= 'A' && c <= 'Z')
                    {
                        found = true;
                        break;
                    }
                if (!found)
                {
                    sum += 3;
                    sum += word.Length;
                }
            }
            return sum;
        }

        private void backMoveButton_Click(object sender, RoutedEventArgs e)
        {
            gameHistory.movePrev();
            fillProductionsTextBlock();
            fillPlayersInfo();
            fillAnotherInfo();

        }

        private void forwardMoveButton_Click(object sender, RoutedEventArgs e)
        {
            gameHistory.moveNext();
            fillProductionsTextBlock();
            fillPlayersInfo();
            fillAnotherInfo();

        }

        private void fullBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            while (gameHistory.hasPrevMove())
                gameHistory.movePrev();
            fillProductionsTextBlock();
            fillPlayersInfo();
            fillAnotherInfo();
        }

        private void fullForwardButton_Click(object sender, RoutedEventArgs e)
        {
            while (gameHistory.hasNextMove())
                gameHistory.moveNext();
            fillProductionsTextBlock();
            fillPlayersInfo();
            fillAnotherInfo();
        }
    }
}
