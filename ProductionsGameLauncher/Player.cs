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
        public delegate Strategy getPlayer();
        public string Name { get; private set; }
        private getPlayer player;

        public Player(string name, getPlayer player)
        {
            Name = name;
            this.player = player;
        }

        public Strategy get() {
            return player();
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
