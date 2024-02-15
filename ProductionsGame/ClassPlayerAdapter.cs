using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGame
{
    public class ClassPlayerAdapter : PlayerAdapter
    {
        public string Name { get; private set; }
        private Strategy strategy {get;set;}

        public ClassPlayerAdapter(int number, GameCompiler gameCompiler, StreamWriter log, string Name,Strategy strategy)
            :base(number, gameCompiler, log) 
        {
            this.Name = Name;
            this.strategy = strategy;
            strategy.firstInit(Settings, PlayerNumber);
        }

        protected override Move CalculateMove(int productionGroupNumber)
        {
            return strategy.makeMove(productionGroupNumber);
        }
    }
}
