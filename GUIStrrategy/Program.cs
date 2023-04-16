using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GUIStrrategy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Bank bank;
            GameSettings settings;
            int number;
            List<List<string>> words;
            int productionGroupNumber;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream inputStream = Console.OpenStandardInput();
            settings = (GameSettings)formatter.Deserialize(inputStream);
            bank = (Bank)formatter.Deserialize(inputStream);
            number = (int)formatter.Deserialize(inputStream);
            words = (List<List<string>>)formatter.Deserialize(inputStream);
            productionGroupNumber = (int)formatter.Deserialize(inputStream);
            //TODO catch + check errors
            
            Move move = new Move();
            move.addMove(0, productionGroupNumber, 0);


            Console.Out.Write(move);
        }
    }
}
