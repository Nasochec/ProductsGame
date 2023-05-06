using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ProductionsGameCore;
using StrategyUtilities;

namespace TestingConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameSettings gameSettings = GameSettings.ReadFromFile(@"./conf1.xml");
            double[] netMetric;
            double[][] prodmetric;

            List<SimplifiedProductionGroup> prods = new List<SimplifiedProductionGroup>();

            for (int i = 0; i < gameSettings.ProductionsCount; ++i)
                prods.Add(new SimplifiedProductionGroup(gameSettings.getProductionGroup(i)));


            StrategyUtilitiesClass.countMetric(prods, gameSettings, out netMetric, out prodmetric);

            SimplifiedWord w = new SimplifiedWord("k");
            w.addNeterminal('A', 1);
            double b = StrategyUtilitiesClass.countWordMetric(w, gameSettings.RandomSettings, netMetric, prods);



            //ProductionGroup A = new ProductionGroup('A',"Aba","jabo","");
            //ProductionGroup B = new ProductionGroup('B',"BA","kurwa");

            //RandomSettings rs = new RandomSettings(5, new int[] { 2, 3 });

            //GameSettings settings = new GameSettings(true,2,3,new ProductionGroup[] {A,B},rs);

            //settings.WriteToFile("out.xml",false);

            //XElement XGameSettings = XElement.Load("out.xml");

            //XElement Xprods = XGameSettings.Element("Productions");
            //Console.WriteLine(Xprods.Elements().Count());

            //GameSettings gs = GameSettings.ReadFromFile("out.xml");
        }
    }
}
