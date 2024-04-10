﻿using ProductionsGame.Properties;
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
        //public int PlayerNumber { get; private set; }
        protected GameSettings GameSettings { get; private set; }
        protected List<ProductionGroup> productions;
        protected List<SimplifiedProductionGroup> simplifiedProductions;
        protected RandomSettings rs;
        private bool initialized = false;

        protected Strategy(string name)
        {
            Name = name;
        }

        static Parameters getParameters()
        {
            return new Parameters();
        }

        public void setGameSettings(GameSettings gameSettings)
        {
            GameSettings = gameSettings;
            productions = this.GameSettings.GetProductions().ToList();
            for (int i = 0; i < GameSettings.ProductionsCount; ++i)
                simplifiedProductions.Add(new SimplifiedProductionGroup(GameSettings.getProductionGroup(i)));
            this.rs = this.GameSettings.RandomSettings;
            GameSettingsChanged.Invoke(this, null);
        }

        /// <summary>
        /// Subscribe this if it is necessary to perform some actions when game settigs change (count metric, etc.).
        /// </summary>
        protected event EventHandler GameSettingsChanged;

        /// <summary>
        /// Метод который надо переопределить чтобы сделать свою стратегию.
        /// </summary>
        /// <param name="productionNumber"></param>
        public abstract Move makeMove(int playerNumber,
            int MoveNumber,
            int productionNumber,
            List<List<string>> words,
            List<List<SimplifiedWord>> simplifiedWords,
            Bank bank);

        public override string ToString()
        {
            return Name;
        }
    }
}
