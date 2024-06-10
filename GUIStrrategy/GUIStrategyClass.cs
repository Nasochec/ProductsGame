using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
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
        public GUIStrategyClass():base() { Name = "GUI stategy"; }


        public override Move makeMove(int playerNumber,
            int MoveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank)
        {
            Move move = new Move();

            //TODO maybe uncomment threads
            //Thread newWindowThread = new Thread(new ThreadStart(() =>
            //{
            // create and show the window
            if (StrategyUtilitiesClass.findMatches(simplifiedWords[playerNumber], simplifiedProductions[productionNumber].Left).Count == 0
                && productions[productionNumber].Left != 'S') 
                return move;
            InputForm form = new InputForm(GameSettings, playerNumber, MoveNumber, bank, words, productionNumber, ref move);
            form.ShowDialog();
            
            //}));

            //// set the apartment state  
            //newWindowThread.SetApartmentState(ApartmentState.STA);

            //// start the thread  
            //newWindowThread.Start();

            ////newWindowThread.Abort();
            //newWindowThread.Join();

            return move;
        }
    }
}
