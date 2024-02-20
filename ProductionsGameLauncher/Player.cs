using ProductionsGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameLauncher
{
    public class Player
    {
        public delegate Strategy getPlayer(Parameters parameters);
        public string Name { get; private set; }
        private getPlayer player;
        public Parameters Parameters { get; private set; }

        public Player(string name, getPlayer player,Parameters parameters = null)
        {
            Name = name;
            this.player = player;
            this.Parameters = parameters;
        }


        public Strategy get() {
            return player(Parameters);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
