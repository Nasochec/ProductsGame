using ProductionsGame.Properties;
using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGame
{
    public abstract class Strategy
    {
        public string Name { get; protected set; }
        public int PlayerNumber { get; private set; }
        protected GameSettings Settings { get; private set; }
        protected List<ProductionGroup> prods;
        protected RandomSettings rs;
        private bool initialized = false;

        protected Strategy(string name)
        {
            Name = name;
        }

        static Parameters getParameters() { 
            return new Parameters();
        }

        /// <summary>
        /// Используется для начальной инициализации.
        /// </summary>
        public void firstInit(GameSettings gameSettings, int playerNumber)
        {
            if (!initialized)
            {
                this.Settings = gameSettings;
                this.PlayerNumber = playerNumber;
                this.initialized = true;
                //this.Bank = new Bank(this.Settings.ProductionsCount);
                //this.words = new List<List<string>>();
                //for (int player = 0; player < this.Settings.NumberOfPlayers; ++player)
                //    this.words.Add(new List<string>());
                prods = this.Settings.GetProductions().ToList();
                this.rs = this.Settings.RandomSettings;
                beforeStart();
            }
        }

        /// <summary>
        /// Переопределить чтобы выполнить действие которое надо выполнить 1 раз перед запуском программы.
        /// </summary>
        protected abstract void beforeStart();

        /// <summary>
        /// Метод который надо переопределить чтобы сделать свою стратегию.
        /// </summary>
        /// <param name="productionNumber"></param>
        public abstract Move makeMove(int productionNumber,int MoveNumber, List<List<string>> words, Bank bank);

        public override string ToString()
        {
            return Name;
        }
    }
}
