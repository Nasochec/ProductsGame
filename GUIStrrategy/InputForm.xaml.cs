using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUIStrategy
{
    /// <summary>
    /// Interaction logic for InputForm.xaml
    /// </summary>
    public partial class InputForm : Window
    {
        private GameSettings gameSettings;
        private Bank bank;
        private int playerNumber;
        private List<List<string>> words;
        private int productionGroupNumber;
        private Move move;
        private int moveNumber;


        List<StrategicPrimaryMove> moves;

        public InputForm(GameSettings gameSetting,
            int playerNumber,
            int moveNumber,
            Bank bank,
            List<List<string>> words,
            int productionGroupNumber,
            ref Move move)
        {

            InitializeComponent();
            this.gameSettings = gameSetting;
            this.playerNumber = playerNumber;
            this.bank = bank;
            this.productionGroupNumber = productionGroupNumber;
            this.words = words;
            this.move = move;
            this.moveNumber = moveNumber;
            this.moves = new List<StrategicPrimaryMove>();
            fillProductionListBox();
            fillPlayerWordsListBox();
            fillTextBlocks();
            fillSelectedProductionGroupListBox();
            fillAvaliableWordsListBox();
            productionsListBox.SelectionChanged += (sender, e) =>
            {
                if (moves.Count >= 1 && productionsListBox.Items.Count > 0)
                {
                    fillSelectedProductionGroupListBox();
                    fillAvaliableWordsListBox();
                }
            };
        }

        private void fillProductionListBox()
        {
            productionsListBox.Items.Clear();
            var productionEnumerator = gameSettings.GetProductions().GetEnumerator();
            for (int productioNumber = 0; productioNumber < gameSettings.ProductionsCount; ++productioNumber)
            {
                productionEnumerator.MoveNext();
                var curr = productionEnumerator.Current;
                TextBlock tb = new TextBlock();
                tb.Text = (productioNumber + 1) + ". Количество: " + bank.getProductionCount(productioNumber) +
                    " " + curr.ToString();
                if ((StrategyUtilitiesClass.findMatches(words[playerNumber], curr.Left).Count != 0 || curr.Left == 'S') &&
                    bank.getProductionCount(productioNumber) > 0)
                {
                    tb.Foreground = Brushes.Green;
                }
                else
                {
                    tb.Foreground = Brushes.Red;
                }
                productionsListBox.Items.Add(tb);
            }
            productionsListBox.SelectionMode = SelectionMode.Single;
            productionsListBox.SelectedIndex = 0;
        }

        private void fillTextBlocks()
        {
            totalMovesTextBlock.Text = "Всего ходов: " + gameSettings.NumberOfMoves; 
            moveNumberTextBlock.Text = "Номер хода: " + (moveNumber + 1);
            playerNumberTextBlock.Text = "Вы игрок под номером: " + (playerNumber + 1);
            productionNumberTextBlock.Text = "Выпала продукция под номером: " + (productionGroupNumber + 1);
        }

        private void fillPlayerWordsListBox()
        {
            //Заполняем слова основного игрока
            mainPlayerWordsListBox.Items.Clear();
            foreach (var word in words[playerNumber])
                mainPlayerWordsListBox.Items.Add(word);
            //заполняем слова противника
            secondaryPlayerWordsListBox.Items.Clear();
            foreach (var word in words[1-playerNumber])
                secondaryPlayerWordsListBox.Items.Add(word);
        }

        private void fillAvaliableWordsListBox()
        {
            int index;
            if (moves.Count >= 1)
                index = productionsListBox.SelectedIndex;
            else
                index = productionGroupNumber;
            ProductionGroup prod = gameSettings.getProductionGroup(index);
            List<int> matches = StrategyUtilitiesClass.findMatches(words[playerNumber], prod.Left);
            avaliablePlayerWordsListBox.Items.Clear();
            foreach (var match in matches)
            {
                avaliablePlayerWordsListBox.Items.Add(words[playerNumber][match]);
            }
            if (prod.Left == 'S')
                avaliablePlayerWordsListBox.Items.Add("<Новое слово>");
            avaliablePlayerWordsListBox.SelectionMode = SelectionMode.Single;
            avaliablePlayerWordsListBox.SelectedIndex = 0;
        }

        private void fillSelectedProductionGroupListBox()
        {
            int index;
            if (moves.Count >= 1)
            {
                index = productionsListBox.SelectedIndex;
                selectedProductionGroupTextBlock.Text = "Вами вырана продукция из банка: ";
            }
            else
            {
                index = productionGroupNumber;
                selectedProductionGroupTextBlock.Text = "Сейчас вы обязаны применить выпавшую продукцию " + (index + 1) + " : ";
            }
            ProductionGroup prod = gameSettings.getProductionGroup(index);
            selectedProductionGroupTextBlock.Text += prod.Left + "->";
            selectedProductionGroupListBox.Items.Clear();
            foreach (var right in prod.getRights())
                selectedProductionGroupListBox.Items.Add(right);
            selectedProductionGroupListBox.SelectedIndex = 0;
            selectedProductionGroupListBox.SelectionMode = SelectionMode.Single;
        }

        private void fillMovesListBox()
        {
            moveListBox.Items.Clear();
            foreach (var move in moves)
            {
                ProductionGroup prod = gameSettings.getProductionGroup(move.ProductionGroupNumber);
                moveListBox.Items.Add(move.PrevWord + " -> " + move.NewWord);
            }
        }

        private void updateListBoxes()
        {
            fillProductionListBox();
            fillAvaliableWordsListBox();
            fillMovesListBox();
            fillPlayerWordsListBox();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (checkMoveCompleted())
            {
                var rez = MessageBox.Show("Вы уверены что хотите завершить ход?", "завершение хода", MessageBoxButton.YesNo);
                if (rez == MessageBoxResult.Yes)
                {
                    StrategicPrimaryMove.toMove(moves, ref move);
                }
                else
                    e.Cancel = true;
            }
            else
            {
                var rez = MessageBox.Show("Вы уверены, что хотите закрыть окно? При нажатии да, будет засчитано что вы сдались.", "Сдаться?", MessageBoxButton.YesNo);
                if (rez != MessageBoxResult.Yes)
                    e.Cancel = true;
                else
                    Environment.Exit(2);
            }
        }

        private void deleteLastMoveButton_Click(object sender, RoutedEventArgs e)
        {
            deleteLastMove();
            updateListBoxes();
        }

        private void deleteLastMove()
        {
            if (moves.Count != 0)
            {
                var move = moves.Last();
                if (move.PrevWord != "")
                {//"Отменяем" продукцию
                    words[playerNumber][move.WordNumber] = move.PrevWord;
                }
                else
                {//удаляем созданное слово
                    words[playerNumber].RemoveAt(move.WordNumber);
                }
                if (moves.Count >= 2)
                {//Возвращаем продукцию в банк
                    bank.addProduction(move.ProductionGroupNumber);
                }
                moves.RemoveAt(moves.Count - 1);
            }
        }

        private void deleteAllMovesButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить все ходы?", "Удалить все ходы?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                while (moves.Count != 0)
                    deleteLastMove();
                updateListBoxes();
            }

        }

        private void addMoveButton_Click(object sender, RoutedEventArgs e)
        {
            int productionGroupIndex;
            if (moves.Count >= 1)
                productionGroupIndex = productionsListBox.SelectedIndex;
            else
                productionGroupIndex = productionGroupNumber;
            int productionIndex = selectedProductionGroupListBox.SelectedIndex;
            int wordIndex = avaliablePlayerWordsListBox.SelectedIndex;

            if (productionGroupIndex < 0 || productionGroupIndex >= gameSettings.ProductionsCount)
            {
                MessageBox.Show("Выбана недопустимая группа продукций.");
                return;
            }
            ProductionGroup prod = gameSettings.getProductionGroup(productionGroupIndex);
            if (productionIndex < 0 || productionIndex >= prod.RightSize)
            {
                MessageBox.Show("Выбана недопустимая продукция в группе.");
                return;
            }

            List<int> allowedIndexes = StrategyUtilitiesClass.findMatches(words[playerNumber], prod.Left);

            if (moves.Count >= 1)
                if (bank.getProductionCount(productionGroupIndex) <= 0)
                {
                    MessageBox.Show("Количество таких продукций в банке равно 0, выберите другую.");
                    return;
                }

            if (wordIndex >= 0 && wordIndex < allowedIndexes.Count)
            {
                wordIndex = allowedIndexes[wordIndex];
                string word = words[playerNumber][wordIndex];
                int letterIndex = StrategyUtilitiesClass.isHaveLetter(word, prod.Left);
                if (letterIndex == -1)
                {
                    MessageBox.Show("Выбрано некорректное слово, оно не сождержит подходящего нетерминального симола.");
                    return;
                }
                string newWord = word.Substring(0, letterIndex) +
                             prod.getRightAt(productionIndex) +
                             word.Substring(letterIndex + 1, word.Length - letterIndex - 1);
                words[playerNumber][wordIndex] = newWord;
                moves.Add(new StrategicPrimaryMove(wordIndex, letterIndex, productionGroupIndex, productionIndex, word, newWord));
            }
            else if (prod.Left == 'S')
            {
                string newWord = prod.getRightAt(productionIndex);
                moves.Add(new StrategicPrimaryMove(words[playerNumber].Count, 0, productionGroupIndex, productionIndex, "", newWord));
                words[playerNumber].Add(newWord);
            }
            else
            {
                MessageBox.Show("Выбрано недопустимое слово!");
                return;
            }
            if (moves.Count >= 2)
            {
                bank.removeProduction(productionGroupIndex);
            }
            updateListBoxes();
        }

        private bool checkMoveCompleted()
        {
            if (moves.Count == 0 && avaliablePlayerWordsListBox.Items.Count == 0 ||
                moves.Count >= 1 && findAvaliableProductions().Count == 0)
                return true;
            return false;
        }

        private List<int> findAvaliableProductions()
        {
            List<int> matches = new List<int>();
            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
            {
                if (bank.getProductionCount(i) > 0)
                {
                    if (StrategyUtilitiesClass.findMatches(
                        words[playerNumber],
                        gameSettings.getProductionGroup(i).Left
                        ).Count != 0 || gameSettings.getProductionGroup(i).Left == 'S')
                    {
                        matches.Add(i);
                    }
                }
            }
            return matches;
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
