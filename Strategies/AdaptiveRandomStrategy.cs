using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductionsGame;
using ProductionsGameCore;

namespace Strategies
{
    public class AdaptiveRandomStrategy: Strategy
    {
        public int changeMoves = 20;
        SmartRandomStrategy smartRandomStrategy;
        ShortWordsStrategy shortWordsStrategy;

        public AdaptiveRandomStrategy(Parameters parameters) : base("Adaptive Random")
        {
            //var param = parameters.getParameter("depth");
            //if (param != null && param.Value >= 0)
            //    maxDepth = param.Value;
            smartRandomStrategy = new SmartRandomStrategy();
            shortWordsStrategy = new ShortWordsStrategy();
            this.GameSettingsChanged += beforeStart;
        }

        
        protected void beforeStart(object sender, EventArgs e) { 
            smartRandomStrategy.setGameSettings(this.GameSettings);
            shortWordsStrategy.setGameSettings(this.GameSettings);
        }
        
        public override Move makeMove(int playerNumber, int MoveNumber, int productionNumber, List<List<string>> words, List<List<SimplifiedWord>> simplifiedWords, Bank bank)
        {
            if(MoveNumber < GameSettings.NumberOfMoves - changeMoves)
                return smartRandomStrategy.makeMove(playerNumber, MoveNumber, productionNumber, words, simplifiedWords, bank);
            else
                return shortWordsStrategy.makeMove(playerNumber, MoveNumber, productionNumber, words, simplifiedWords, bank);

        }
        //new public static Parameters getParameters()
        //{
        //    return new Parameters();
        //}
    }
}
