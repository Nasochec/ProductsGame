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

namespace GUIStrategy
{
    /// <summary>
    /// Interaction logic for WordsWindow.xaml
    /// </summary>
    public partial class WordsWindow : Window
    {
        private List<string> words = new List<string>();
        private int playerNumber;
        public WordsWindow(List<string> words, int playerNumber)
        {
           
            InitializeComponent();
            this.words = words;
            this.playerNumber = playerNumber;
            this.Title = "Слова игрока " + (playerNumber + 1);
            playerNumberTextBox.Text = "Слова игрока " + (playerNumber + 1) + ":";
            fillPlayerWordsListBox();
        }

        private void fillPlayerWordsListBox()
        {
            playerWordsListBox.Items.Clear();
            foreach (string word in words)
            {
                playerWordsListBox.Items.Add(word);
            }
        }
    }
}
