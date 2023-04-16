using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


namespace ProductsGame
{
    public class ExeSerializationPlayerAdapter : PlayerAdapter
    {
        String filename;
        private Process player;
        public ExeSerializationPlayerAdapter(int number,
            GameCompiler gameCompiler,
            String filename,
            string logFilename)
            : base(number,gameCompiler,logFilename)
        {
            this.filename = filename;
        }

        override protected Move CalculateMove(int productionGroupNumber)
        {
            //Получим список всех слов всех пользователей для передачи в программы стратегий.
            List<List<string>> playersWords = GameCompiler.getPlayersWords();

            player = new Process();
            player.StartInfo.FileName = filename;
            player.StartInfo.CreateNoWindow = true;
            player.StartInfo.RedirectStandardInput = true;
            player.StartInfo.RedirectStandardOutput = true;
            player.StartInfo.UseShellExecute = false;
            //player.StartInfo.WorkingDirectory = filename;
            player.Start();
           
            //Направляем программе на вход данные.
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(player.StandardInput.BaseStream, Settings);
            formatter.Serialize(player.StandardInput.BaseStream, Bank);
            formatter.Serialize(player.StandardInput.BaseStream, MyNumber);
            formatter.Serialize(player.StandardInput.BaseStream, playersWords);
            formatter.Serialize(player.StandardInput.BaseStream, productionGroupNumber);

            player.StandardInput.Flush();
            player.StandardInput.Close();
            player.WaitForExit(5000);//TODO if not finished in this time - end
            string output = player.StandardOutput.ReadToEnd();
            Move move = Move.FromString(output);
            return move;
        }
    }
}
