using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

//[DllImport("kernel32.dll")]
//static extern IntPtr GetConsoleWindow();

//[DllImport("user32.dll")]
//static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

//const int SW_HIDE = 0;
//const int SW_SHOW = 5;


namespace GUIStrrategy
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Bank bank;
            GameSettings settings;
            int number;
            List<List<string>> words;
            int productionGroupNumber;
            int moveNumber;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream inputStream = Console.OpenStandardInput();
            settings = (GameSettings)formatter.Deserialize(inputStream);
            number = (int)formatter.Deserialize(inputStream);

            //TODO - delete
            //settings = GameSettings.ReadFromFile(@"./testing.xml");
            //number = 0;
            //bank = new Bank(settings.ProductionsCount);
            //bank.addProduction(0);
            //bank.addProduction(0);
            //words = new List<List<string>>();
            //words.Add(new List<string>());
            //words[0].Add("SaSat");
            //words.Add(new List<string>());
            //productionGroupNumber = 0;

            for (int i = 0; i < settings.NumberOfMoves; i++)
            {
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);
                //TODO catch + check errors

                Move move = new Move();

                Thread newWindowThread = new Thread(new ThreadStart(() =>
                {
                    // create and show the window
                    InputForm form = new InputForm(settings, number, moveNumber, bank, words, productionGroupNumber, ref move);
                    form.ShowDialog();
                }));

                // set the apartment state  
                newWindowThread.SetApartmentState(ApartmentState.STA);

                // make the thread a background thread  
                // newWindowThread.IsBackground = true;
                //newWindowThread.IsBackground = false;

                // start the thread  
                newWindowThread.Start();

                //newWindowThread.Abort();
                newWindowThread.Join();

                Console.Out.WriteLine(move);
            }
            return 0;
        }
    }
}
