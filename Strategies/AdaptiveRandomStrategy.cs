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
        Strategy startingStrategy;
        Strategy finishingStrategy;

        public AdaptiveRandomStrategy() : base()
        {
            Name = "Adaptive Random";
            ShortName = "AR";
            startingStrategy = new InversedSmartRandomStrategy();
            //finishingStrategy = new ShortWordsStrategy();
            var parameters = SearchStrategy.getParameters();
            finishingStrategy = new SearchStrategy(parameters);

            this.GameSettingsChanged += beforeStart;
        }

        
        protected void beforeStart(object sender, EventArgs e) { 
            startingStrategy.setGameSettings(this.GameSettings);
            finishingStrategy.setGameSettings(this.GameSettings);
        }

        protected int countNonterminals(List<SimplifiedWord> words) {
            int count = 0;
            foreach (SimplifiedWord word in words)
            {
                foreach (var nonterminal in word.nonterminals)
                    count += nonterminal.Value;
            }
            return count;
        }
        
        public override Move makeMove(int playerNumber, int moveNumber, int productionNumber, List<List<string>> words, List<List<SimplifiedWord>> simplifiedWords, Bank bank)
        {
            //TODO make smarter strategy shange 
            int nonterminals = countNonterminals(simplifiedWords[playerNumber]);
            //if(MoveNumber < GameSettings.NumberOfMoves - changeMoves)
            if (nonterminals * 2 + changeMoves < GameSettings.NumberOfMoves - moveNumber )//|| nonterminals >= 10
                return startingStrategy.makeMove(playerNumber, moveNumber, productionNumber, words, simplifiedWords, bank);
            else
                return finishingStrategy.makeMove(playerNumber, moveNumber, productionNumber, words, simplifiedWords, bank);

        }
        //new public static Parameters getParameters()
        //{
        //    return new Parameters();
        //}
    }
}
