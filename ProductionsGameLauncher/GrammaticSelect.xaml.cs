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
    /// Interaction logic for SettingsSelect.xaml
    /// </summary>
    public partial class SettingsSelect : Window
    {
        List<Grammatic> grammatics = new List<Grammatic>();
        public Grammatic Result {get;private set;}

        public SettingsSelect(List<Grammatic> grammatics)
        {
            InitializeComponent();
            this.grammatics = grammatics;
            grammaticsListBox.SelectionMode = SelectionMode.Single;
            grammaticsListBox.SelectionChanged += (o, args) =>
            {
                if (args.RemovedItems.Count != 0)
                {
                    grammaticTextBlock.Text = "";
                }
                if (args.AddedItems.Count != 0)
                {
                    int index = grammaticsListBox.SelectedIndex;
                    grammaticTextBlock.Text = grammatics[index].ToString();
                }
            };
            fillListBox();
        }

        private void fillListBox() {
            grammaticsListBox.Items.Clear();
            foreach (var grammatic in grammatics) {
                grammaticsListBox.Items.Add(grammatic.Name);
            }
            grammaticsListBox.SelectedIndex = 0;
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            Result = grammatics[grammaticsListBox.SelectedIndex];
            this.DialogResult = true;
            this.Close();
        }

        private void createGrammaticButton_Click(object sender, RoutedEventArgs e)
        {
            CreateGrammatic createGrammatic = new CreateGrammatic();
            bool? res= createGrammatic.ShowDialog();
            if (res == true) {
                grammatics.Add(createGrammatic.Result);
                fillListBox();
            }
        }
    }
}
