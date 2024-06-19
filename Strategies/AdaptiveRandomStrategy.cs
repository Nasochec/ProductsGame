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
        //public int changeMoves = 20;
        private double changeStrategyCoef = 10d;
        Strategy startingStrategy;
        Strategy finishingStrategy;

        public AdaptiveRandomStrategy(Parameters parameters) : base()
        {
            startingStrategy = new InversedSmartRandomStrategy();
            //startingStrategy = new ImprovedRandomStrategy();
            finishingStrategy = new SearchStrategy(parameters);
            //finishingStrategy = new ShortWordsStrategy();
            if (parameters != null)
            {
                var param = parameters.getParameter("finish_coef");
                if (param != null && param is DoubleParameter && (param as DoubleParameter).Value >= 0)
                    changeStrategyCoef = (param as DoubleParameter).Value;
            }
            this.GameSettingsChanged += beforeStart;
            Name = "Adaptive Random "+changeStrategyCoef.ToString();
            ShortName = "AR" + changeStrategyCoef.ToString();
        }

        
        protected void beforeStart(object sender, EventArgs e) { 
            startingStrategy.setGameSettings(this.GameSettings);
            finishingStrategy.setGameSettings(this.GameSettings);
        }

        public static new Parameters getParameters()
        {
            Parameters searchParameters = new Parameters();
            searchParameters.addParameter(new DoubleParameter("finish_coef", "Коэффициент смены стратегии", 10d));
            return searchParameters;
        }

        protected int countNonterminals(List<SimplifiedWord> words) {
            int count = 0;
            foreach (SimplifiedWord word in words)
                for(int nonterminal=0;nonterminal<word.NonterminalsCount;++nonterminal)
                    count += word[nonterminal];
            return count;
        }
        
        public override Move makeMove(int playerNumber, int moveNumber, int productionNumber, List<List<string>> words, List<List<SimplifiedWord>> simplifiedWords, Bank bank)
        {
            //TODO make smarter strategy shange 
            int nonterminals = countNonterminals(simplifiedWords[playerNumber]);
            //if(MoveNumber < GameSettings.NumberOfMoves - changeMoves)+ changeMoves + 20 
            if (nonterminals * changeStrategyCoef < GameSettings.NumberOfMoves - moveNumber )//|| nonterminals >= 10
                return startingStrategy.makeMove(playerNumber, moveNumber, productionNumber, words, simplifiedWords, bank);
            else
                return finishingStrategy.makeMove(playerNumber, moveNumber, productionNumber, words, simplifiedWords, bank);

        }
    }
}
