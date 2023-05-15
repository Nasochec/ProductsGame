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


        public LookGame(GameCompiler gc)
        {
            InitializeComponent();
            gameHistory = new CompilerGameHistory(gc);
            firstInit();
        }

        private void firstInit()
        {
            totalMovesTextBlock.Text = "Всего ходов:" + gameHistory.GameSettings.NumberOfMoves;
            playersListBoxes[0] = firstPlayerWordsListBox;
            playersListBoxes[1] = secondplayerWordsListBox;
            playersScores[0] = firstPlayerScoreTextBlock;
            playersScores[1] = secondPlayerScoreTextBlock;
            fillProductionsTextBlock();
            fillPlayersInfo();
        }
        //TODO добавить подсчёт текущих очков, добавить сообщение что игра завершена, добавить GUISTRATEGY, отображени текущего хода
        private void fillProductionsTextBlock()
        {
            StringBuilder sb = new StringBuilder();
            var prod = gameHistory.GameSettings.GetProductions().GetEnumerator();
            var count = gameHistory.getCurrentBank().GetEnumerator();
            prod.MoveNext();
            count.MoveNext();
            while (prod.MoveNext() && count.MoveNext())
            {
                sb.AppendLine(count.Current + " " + prod.Current.ToString());
            }
            productionsTextBlock.Text = sb.ToString();
        }

        private void fillPlayersInfo()
        {
            currentMoveTextBlock.Text = "Ход " + (gameHistory.currentMoveNumber + 1);
            currentPlayerTextBlock.Text = "Сходил игрок: " + (gameHistory.currentPlayer + 1);
            productionTextBlock.Text = "Выпала продукция " + gameHistory.currentProductionGroup;

            for (int player = 0; player < 2; ++player)
            {
                playersListBoxes[player].Items.Clear();
                foreach (var word in gameHistory.getPlayerWords(player))
                {
                    playersListBoxes[player].Items.Add(word);
                }
                playersScores[player].Text = "Очки " + (player + 1) + " игрока: " + countScore(gameHistory.getPlayerWords(player));
            }
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

        public LookGame(string filename)
        {
            InitializeComponent();
            gameHistory = new FileGameHistory(filename);
            firstInit();
        }

        private void backMoveButton_Click(object sender, RoutedEventArgs e)
        {
            gameHistory.movePrev();
            fillProductionsTextBlock();
            fillPlayersInfo();
        }

        private void forwardMoveButton_Click(object sender, RoutedEventArgs e)
        {
            gameHistory.moveNext();
            fillProductionsTextBlock();
            fillPlayersInfo();
        }
    }
}
