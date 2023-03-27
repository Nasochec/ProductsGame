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
            GameSettings settings,
            Bank bank,
            IEnumerable<IWords> playersWords,
            String filename)
            : base(number, settings, bank, playersWords)
        {
            this.filename = filename;
            //player = new Process();
            //player.StartInfo.FileName = filename;
            //player.StandardInput
            // player.StandardOutput
            // player.StartInfo.UseShellExecute = true;
            //player.Start();
            //player.WaitForExit();
        }

        override protected Move CalculateMove(int productionGroupNumber, IEnumerable<KeyValuePair<ProductionGroup, int>> bank)
        {
            player = new Process();
            player.StartInfo.FileName = filename;
            player.StartInfo.CreateNoWindow = true;
            player.StartInfo.RedirectStandardInput = true;
            player.StartInfo.RedirectStandardOutput = true;
            player.StartInfo.UseShellExecute = false;
            //player.StartInfo.WorkingDirectory = filename;
            player.Start();
            
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(player.StandardInput.BaseStream,Settings);
            
            
            player.StandardInput.Write();//Write current config 



            player.StandardInput.Flush();
            player.StandardInput.Close();
            player.WaitForExit(5000);//TODO if not finished in this time - end
            string output = player.StandardOutput.ReadToEnd();
            Move move = Move.FromString(output);
            return move;
        }
    }
}
