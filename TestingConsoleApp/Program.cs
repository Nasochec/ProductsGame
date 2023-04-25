using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ProductionsGameCore;

namespace TestingConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ProductionGroup A = new ProductionGroup('A',"Aba","jabo","");
            ProductionGroup B = new ProductionGroup('B',"BA","kurwa");

            RandomSettings rs = new RandomSettings(5, new int[] { 2, 3 });
            
            GameSettings settings = new GameSettings(true,2,3,new ProductionGroup[] {A,B},rs);

            settings.WriteToFile("out.xml",false);

            XElement XGameSettings = XElement.Load("out.xml");

            XElement Xprods = XGameSettings.Element("Productions");
            Console.WriteLine(Xprods.Elements().Count());

            //GameSettings gs = GameSettings.ReadFromFile("out.xml");
        }
    }
}
