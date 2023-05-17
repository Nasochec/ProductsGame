using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;


namespace GUIStrategy
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

            if (settings.NumberOfPlayers != 2) return 1;

            for (int i = 0; i < settings.NumberOfMoves; i++)
            {
                moveNumber = (int)formatter.Deserialize(inputStream);
                bank = (Bank)formatter.Deserialize(inputStream);
                words = (List<List<string>>)formatter.Deserialize(inputStream);
                productionGroupNumber = (int)formatter.Deserialize(inputStream);

                Move move = new Move();

                Thread newWindowThread = new Thread(new ThreadStart(() =>
                {
                    // create and show the window
                    InputForm form = new InputForm(settings, number, moveNumber, bank, words, productionGroupNumber, ref move);
                    form.ShowDialog();
                }));

                // set the apartment state  
                newWindowThread.SetApartmentState(ApartmentState.STA);

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
