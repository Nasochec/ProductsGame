using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Strategies
{
    public class SmartRandomStrategy:RandomStrategy
    {
        protected double[] netMetric;
        protected double[][] prodsMetric;

        public SmartRandomStrategy():base() {
            Name = "Smart Random Strategy";
            ShortName = "SRS";
            this.GameSettingsChanged += beforeStart;
        }

        protected virtual void beforeStart(object sender, EventArgs e) {
            StrategyUtilitiesClass.countMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
        }

        /// <summary>
        /// Counts weights of production groups.
        /// override this method for new strategy
        /// </summary>
        protected override List<double> getGroupsWeights(List<int> indexes)
        {
            return indexes.Select((index) => netMetric[index]).ToList();
            return indexes.Select((index) => Math.Sqrt(netMetric[index])).ToList();
        }
        /// <summary>
        /// Counts weights of productions in production groups.
        /// </summary>
        protected override List<double> getProductionsWeights(int index)
        {
            return prodsMetric[index].Select((m) => m).ToList();
            return prodsMetric[index].Select((m)=> Math.Sqrt(m)).ToList();
        }
        /// <summary>
        /// Counts weights of words.
        /// </summary>
        protected override List<double> getWordsWeights(List<SimplifiedWord> words)
        {
            return words.Select(
                (word) =>
                StrategyUtilitiesClass.countWordMetric(word, rs, netMetric, simplifiedProductions)
                //Math.Sqrt(StrategyUtilitiesClass.countWordMetric(word, rs, netMetric, simplifiedProductions))
                ).ToList();
        }
    }

    public class ImprovedRandomStrategy : RandomStrategy
    {
        protected double[] netMetric;
        protected double[][] prodsMetric;
        protected double[] netStupidMetric;
        protected double[][] prodsStupidMetric;

        public ImprovedRandomStrategy() : base()
        {
            Name = "Improved Random Strategy";
            ShortName = "IRS";
            this.GameSettingsChanged += beforeStart;
        }

        protected virtual void beforeStart(object sender, EventArgs e)
        {
            StrategyUtilitiesClass.countMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
            StrategyUtilitiesClass.countMetric(simplifiedProductions, rs, out netStupidMetric, out prodsStupidMetric);
        }

        /// <summary>
        /// Counts weights of production groups.
        /// override this method for new strategy
        /// </summary>
        protected override List<double> getGroupsWeights(List<int> indexes)
        {
            return indexes.Select((index) =>{
                if (netMetric[index] == 0 )
                    return 0;
                else if(netMetric[index] == 1)
                    return 0.01;
                else
                    return netStupidMetric[index];
            }).ToList();
        }
        /// <summary>
        /// Counts weights of productions in production groups.
        /// </summary>
        protected override List<double> getProductionsWeights(int index)
        {
            List<double> weights = new List<double>();
            for (int i = 0; i < simplifiedProductions[index].RightSize; i++)
            {
                if (prodsMetric[index][i] == 0)
                    weights.Add(0);
                else if (prodsMetric[index][i] == 1)
                    weights.Add(0.01);
                else
                    weights.Add(prodsStupidMetric[index][i]);
            }
            return weights;
        }
        /// <summary>
        /// Counts weights of words.
        /// </summary>
        protected override List<double> getWordsWeights(List<SimplifiedWord> words)
        {
            return words.Select((word) => {
                double m = StrategyUtilitiesClass.countWordMetric(word, rs, netMetric, simplifiedProductions);
                if (m == 0)
                    return 0d;
                else if (m == 1)
                    return 0.01;
                else
                    return StrategyUtilitiesClass.countWordStupidMetric(word);
            }).ToList();
        }
    }

    public class BetterSmartRandomStrategy : SmartRandomStrategy
    {

        public BetterSmartRandomStrategy() : base()
        {
            Name = "Better Smart Random Strategy";
            ShortName = "BSRS";
        }

        protected override void beforeStart(object sender, EventArgs e)
        {
            StrategyUtilitiesClass.countBetterMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
        }

        /// <summary>
        /// Counts weights of production groups.
        /// override this method for new strategy
        /// </summary>
        protected override List<double> getGroupsWeights(List<int> indexes)
        {
            return indexes.Select((index) => Math.Sqrt(netMetric[index])).ToList();
        }
        /// <summary>
        /// Counts weights of productions in production groups.
        /// </summary>
        protected override List<double> getProductionsWeights(int index)
        {
            return prodsMetric[index].Select((m) => Math.Sqrt(m)).ToList();
        }
        /// <summary>
        /// Counts weights of words.
        /// </summary>
        protected override List<double> getWordsWeights(List<SimplifiedWord> words)
        {
            return words.Select(
                (word) =>
                Math.Sqrt(StrategyUtilitiesClass.countWordMetric(word, rs, netMetric, simplifiedProductions))
                ).ToList();
        }
    }

    public class InversedSmartRandomStrategy : SmartRandomStrategy
    {

        public InversedSmartRandomStrategy() : base()
        {
            Name = "Inversed Smart Random Strategy";
            ShortName = "ISRS";
        }

        protected override void beforeStart(object sender, EventArgs e)
        {
            StrategyUtilitiesClass.countBetterMetric(simplifiedProductions, rs, out netMetric, out prodsMetric);
        }

        /// <summary>
        /// Counts weights of production groups.
        /// override this method for new strategy
        /// </summary>
        protected override List<double> getGroupsWeights(List<int> indexes)
        {
            return indexes.Select((index) => 
            { 
                if (netMetric[index] == 0)
                    return 0d;
                else
                    return 1.01d - netMetric[index];
            }).ToList();
        }
        /// <summary>
        /// Counts weights of productions in production groups.
        /// </summary>
        protected override List<double> getProductionsWeights(int index)
        {
            return prodsMetric[index].Select((m) => {
                if (m == 0 )//|| m == 1
                    return 0d;
                else
                    return 1.01d - netMetric[index];
            }).ToList();

        }
        /// <summary>
        /// Counts weights of words.
        /// </summary>
        protected override List<double> getWordsWeights(List<SimplifiedWord> words)
        {
            return words.Select((word) => {
                double m = StrategyUtilitiesClass.countBetterWordMetric(word, rs, netMetric, simplifiedProductions);
                if (m == 0)//|| m == 1
                    return 0d;
                else
                    return 1.01d - m;
            }).ToList();
        }
    }
}
