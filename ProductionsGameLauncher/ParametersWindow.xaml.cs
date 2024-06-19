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
                if (parameter is IntParameter)
                {
                    IntParameter p = (IntParameter)parameter;
                    textBox.Text = p.Value.ToString();
                    textBox.PreviewTextInput += (sender, e) =>
                    {
                        string s = (sender as TextBox).Text + e.Text;
                        int rez;
                        bool k = int.TryParse(s, out rez);
                        if(!k)
                            e.Handled = true;
                        //foreach (char c in s)
                        //    if (!(c >= '0' && c <= '9'))
                        //        e.Handled = true;
                    };
                    textBox.TextChanged += (sender, e) =>
                    {
                        if (textBox.Text.Length == 0)
                        {
                            p.Value = 0;
                            return;
                        }
                        p.Value = Convert.ToInt32(textBox.Text);
                    };
                }
                else if (parameter is DoubleParameter) {
                    DoubleParameter p = (DoubleParameter)parameter;
                    textBox.Text = p.Value.ToString();
                    textBox.PreviewTextInput += (sender, e) =>
                    {
                        string s = (sender as TextBox).Text + e.Text;
                        double rez;
                        bool k = double.TryParse(s, out rez);
                        if (!k)
                            e.Handled = true;
                    };
                    textBox.TextChanged += (sender, e) =>
                    {
                        if (textBox.Text.Length == 0)
                        {
                            p.Value = 0;
                            return;
                        }
                        p.Value = Convert.ToDouble(textBox.Text);
                    };
                }
                Grid.SetRow(label,row);
                Grid.SetColumn(label, 0);
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, 1);
                MainGrid.Children.Add(label);
                MainGrid.Children.Add(textBox);
               ++row;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
