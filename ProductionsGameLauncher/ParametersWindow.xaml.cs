using ProductionsGame;
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
    /// Interaction logic for ParametersWindow.xaml
    /// </summary>
    public partial class ParametersWindow : Window
    {
        Parameters parameters;
        public ParametersWindow(Parameters parameters)
        {
            InitializeComponent();
            this.parameters = parameters;
            fillGrid();
        }

        private void fillGrid()
        {
            var parameters = this.parameters.getParameters();
            int row = 0;
            foreach (var parameter in parameters)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(40);
                MainGrid.RowDefinitions.Add(rowDefinition);
                Label label = new Label();
                label.Content = parameter.Name;
                TextBox textBox = new TextBox();
                textBox.Text = parameter.Value.ToString();
                textBox.PreviewTextInput += (sender, e) =>
                {
                    string s = e.Text;
                    foreach (char c in s)
                        if (!(c >= '0' && c <= '9'))
                            e.Handled = true;
                };
                textBox.TextChanged += (sender, e) =>
                {
                    if (textBox.Text.Length == 0)
                    {
                        parameter.Value = 0;
                        return;
                    }
                    parameter.Value = Convert.ToInt32(textBox.Text);
                };
                Grid.SetRow(label,row);
                Grid.SetColumn(label, 0);
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, 1);
                MainGrid.Children.Add(label);
                MainGrid.Children.Add(textBox);
               ++row;
            }
        }
    }
}
