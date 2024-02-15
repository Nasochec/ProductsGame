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
        public int MoveNumber
        {
            get;
            private set;
        }
        protected List<List<string>> words;
        public int PlayerNumber { get; private set; }
        protected GameSettings Settings { get; private set; }
        protected Bank Bank { get; private set; }
        protected List<ProductionGroup> prods;

        private bool initialized = false;

        public Strategy()
        {
            MoveNumber = 0;
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
                this.Bank = new Bank(this.Settings.ProductionsCount);
                this.words = new List<List<string>>();
                for (int player = 0; player < this.Settings.NumberOfPlayers; ++player)
                    this.words.Add(new List<string>());
                prods = this.Settings.GetProductions().ToList();
            }
        }

        /// <summary>
        /// Метод который меняет состояние стратегии.
        /// Можно использовать при создании смешанных стратегий.
        /// Метод безопасен для данных - создаёт их копию чтобы не перезаписать их в процессе работы алгоритма.
        /// </summary>
        public void setState(int MoveNumber, List<List<string>> words, Bank bank)
        {
            if (!initialized)
                throw new Exception("firstInit() must be called before setState()");
            this.MoveNumber = MoveNumber;
            this.words = new List<List<string>>();
            //make copies of collections
            foreach (var playerwords in words)
            {
                // this must make a copy
                this.words.Add(playerwords.AsEnumerable().ToList());
                //var lst = new List<string>();
                //foreach (var word in playerwords)
                //{
                //    lst.Add(word);
                //}
                //this.words.Add(lst);
            }
            for (int i = 0; i < Settings.ProductionsCount; ++i)
                this.Bank.addProduction(i, bank.getProductionCount(i) - this.Bank.getProductionCount(i));
        }

        /// <summary>
        /// Метод который надо переопределить чтобы сделать свою стратегию.
        /// </summary>
        /// <param name="productionNumber"></param>
        public abstract Move makeMove(int productionNumber);

    }
}
