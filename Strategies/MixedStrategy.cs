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
            int randomProb = 1;
            int searchProb = 1;
            int shortProb = 1;
            int adaptiveProb = 1;
            if (parameters != null)
            {
                var param = parameters.getParameter("randomProb");
                if (param != null && param is IntParameter && (param as IntParameter).Value >= 0)
                    randomProb = (param as IntParameter).Value;
                param = parameters.getParameter("searchProb");
                if (param != null && param is IntParameter && (param as IntParameter).Value >= 0)
                    searchProb = (param as IntParameter).Value;
                param = parameters.getParameter("shortProb");
                if (param != null && param is IntParameter && (param as IntParameter).Value >= 0)
                    shortProb = (param as IntParameter).Value;
                param = parameters.getParameter("adaptiveProb");
                if (param != null && param is IntParameter && (param as IntParameter).Value >= 0)
                    adaptiveProb = (param as IntParameter).Value;
            }
            probs.Add(randomProb);
            probs.Add(searchProb);
            probs.Add(shortProb);
            probs.Add(adaptiveProb);
            sum = randomProb + searchProb + shortProb + adaptiveProb;
            strats.Add(new BetterSmartRandomStrategy());
            strats.Add(new SearchStrategy(parameters));
            strats.Add(new ShortWordsStrategy());
            strats.Add(new AdaptiveRandomStrategy(parameters));
            random = new Random((int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId);
            this.GameSettingsChanged += beforeStart;
            Name = string.Format("Mixed Strategy {0} {1} {2} {3}",randomProb, shortProb, searchProb, adaptiveProb);
            ShortName = string.Format("MS{0}{1}{2}{3}", randomProb, shortProb, searchProb, adaptiveProb);

        }

        public static new Parameters getParameters() {
            Parameters mixedParameters = new Parameters();
            //mixedParameters.addParameter(new IntParameter("depth", "Глубина перебора", 4));
            mixedParameters.addParameter(new IntParameter("randomProb", "Вес умной случайной стратегии", 1));
            mixedParameters.addParameter(new IntParameter("shortProb", "Вес стратегии коротких слов", 1));
            mixedParameters.addParameter(new IntParameter("searchProb", "Вес переборной стратегии", 1));
            mixedParameters.addParameter(new IntParameter("adaptiveProb", "Вес адаптивной случаной стратегии", 1));
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
