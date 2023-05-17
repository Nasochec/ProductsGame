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
    /// Interaction logic for CreateGrammatic.xaml
    /// </summary>
    public partial class CreateGrammatic : Window
    {
        public Grammatic Result { get; private set; }
        public CreateGrammatic()
        {
            InitializeComponent();
            Result = null;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Result != null && Result.getProductionGroups().Count() == 11)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            string[] ss = inputTextBox.Text.Split('\n');
            deleteR(ref ss);
            try
            {
                Result = new Grammatic("custom", ss);
                previewTextBlock.Text = Result.ToString();
            }
            catch
            {
                StringBuilder sb = new StringBuilder();
                foreach (string s in ss)
                {
                    try
                    {
                        sb.AppendLine(ProductionGroup.fromString(s).ToString());
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine(ex.Message);
                    }
                }
                previewTextBlock.Text = sb.ToString();
            }
            if (Result != null && Result.getProductionGroups().Count() != 11)
                previewTextBlock.Text += "Количество продукций должно быть равно 11";
        }

        /// <summary>
        /// Удалаяет символ \r из концов строк
        /// </summary>
        /// <param name="ss"></param>
        private void deleteR(ref string[] ss)
        {
            for (int i = 0; i < ss.Length; ++i)
                if (ss[i].Length > 0 && ss[i].Last() == '\r')
                    ss[i] = ss[i].Substring(0, ss[i].Length - 1);
        }


    }
}
