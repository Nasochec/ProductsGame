using ProductionsGame;
using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUIStrategy
{
    public class GUIStrategyClass: Strategy
    {
        public GUIStrategyClass():base("GUI stategy") { }

        protected override void beforeStart()
        {
            if (Settings.NumberOfPlayers != 2) 
                throw new Exception("For using GUI strategy, set number of players in settings to 2.");
        }

        public override Move makeMove(int productionNumber, int MoveNumber, List<List<string>> words, Bank bank)
        {
            Move move = new Move();

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                // create and show the window
                InputForm form = new InputForm(Settings, PlayerNumber, MoveNumber, bank, words, productionNumber, ref move);
                form.ShowDialog();
            }));

            // set the apartment state  
            newWindowThread.SetApartmentState(ApartmentState.STA);

            // start the thread  
            newWindowThread.Start();

            //newWindowThread.Abort();
            newWindowThread.Join();

            return move;
        }
    }
}
