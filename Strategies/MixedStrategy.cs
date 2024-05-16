using ProductionsGame;
using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Strategies
{
    public class MixedStrategy : Strategy
    {
        List<Strategy> strats = new List<Strategy>();
        List<int>probs = new List<int>();
        int sum;
        Random random;


        public MixedStrategy(Parameters parameters) : base()
        {
            Name = "Mixed Strategy";
            ShortName = "MS";
            int randomProb = 1;
            int searchProb = 1;
            int shortProb = 1;
            var param = parameters.getParameter("randomProb");
            if (param != null && param.Value >= 0)
                randomProb = param.Value;
            param = parameters.getParameter("searchProb");
            if (param != null && param.Value >= 0)
                searchProb = param.Value;
            param = parameters.getParameter("shortProb");
            if (param != null && param.Value >= 0)
                shortProb = param.Value;
            probs.Add(randomProb);
            probs.Add(searchProb);
            probs.Add(shortProb);
            sum = randomProb + searchProb + shortProb;
            strats.Add(new RandomStrategy());
            strats.Add(new SearchStrategy(parameters));
            strats.Add(new ShortWordsStrategy());
            random = new Random((int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId);
            this.GameSettingsChanged += beforeStart;

        }

        public static new Parameters getParameters() {
            Parameters mixedParameters = new Parameters();
            mixedParameters.addParameter("depth", "Глубина перебора", 4);
            mixedParameters.addParameter("randomProb", "Вес случайной стратегии", 1);
            mixedParameters.addParameter("searchProb", "Вес переборной стратегии", 1);
            mixedParameters.addParameter("shortProb", "Вес стратегии коротких слов", 1);
            return mixedParameters;
        }

        protected void beforeStart(object sender,EventArgs e)
        {
            foreach(var strat in strats)
                strat.setGameSettings(GameSettings);
        }

        public override Move makeMove(int playerNumber,
            int moveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank)
        {
            int rez = random.Next(sum);
            for (int i = 0; i < strats.Count; ++i) {
                rez -= probs[i];
                if (rez < 0)
                    return strats[i].makeMove(playerNumber,moveNumber,productionNumber,words,simplifiedWords,bank);
            }
            return new Move();
        }
    }
}
