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
        public string Name { get; private set; }
        public string Filename { get; private set; }

        public Player(string name, string filename)
        {
            Name = name;
            Filename = filename;
            if (!File.Exists(filename))
            {
                throw new Exception("Указанный файл отсутствует: " + filename + ".");
            }
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
