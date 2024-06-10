using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProductionsGameCore
{
    public class GameSettings
    {
        private List<ProductionGroup> productions = new List<ProductionGroup>();
        private List<SimplifiedProductionGroup> simplifiedProductions;
        public Simplifier Simplifier { get; private set; }
        public RandomSettings RandomSettings { get; private set; }
        public int NumberOfMoves { get; private set; }
        public int ProductionsCount { get { return productions.Count; } }

        public GameSettings(int numberOfMoves,
            IEnumerable<ProductionGroup> productions,
            RandomSettings randomSettings)
        {
            if (numberOfMoves <= 0)
                throw new ArgumentException("Number of moves must be non-nagative number.");
            NumberOfMoves = numberOfMoves;
            this.productions = productions.ToList();
            RandomSettings = randomSettings;
            Simplifier = new Simplifier(this.productions);
            simplifiedProductions = Simplifier.ConvertProductions(this.productions);

        }

        public GameSettings(int numberOfMoves,
            Grammatic grammatic,
            RandomSettings randomSettings)
            :this(numberOfMoves,grammatic.getProductions(),randomSettings)
        {
        }

        public ProductionGroup getProductionGroup(int index)
        {
            if (index >= 0 && index < ProductionsCount)
                return productions[index];
            throw new IndexOutOfRangeException(
                String.Format("Index {0} was outside of [0,{1}).", index, ProductionsCount)
                );
        }

        public SimplifiedProductionGroup getSimplifiedProductionGroup(int index)
        {
            if (index >= 0 && index < ProductionsCount)
                return simplifiedProductions[index];
            throw new IndexOutOfRangeException(
                String.Format("Index {0} was outside of [0,{1}).", index, ProductionsCount)
                );
        }

        public IEnumerable<ProductionGroup> GetProductions()
        {
            return productions.AsEnumerable();
        }

        public IEnumerable<SimplifiedProductionGroup> GetSimplifiedProductions()
        {
            return simplifiedProductions.AsEnumerable();
        }

        public static GameSettings ReadFromFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return ReadFromStream(fs);
            }
        }

        public static GameSettings ReadFromStream(Stream stream)
        {
            XElement XGameSettings = null;
            try
            {
                XGameSettings = XElement.Load(stream);
            }
            catch
            {
                throw new IOException("Input in wrong format.");
            }
            int numberOfMoves = int.Parse(XGameSettings.Attribute("NumberOfMoves").Value);

            XElement XProductions = XGameSettings.Element("Productions");
            List<ProductionGroup> productions = new List<ProductionGroup>();
            foreach (var XProduction in XProductions.Elements())
            {
                char left = XProduction.Attribute("Left").Value[0];
                List<string> rights = new List<string>();
                foreach (var XRight in XProduction.Elements())
                    rights.Add(XRight.Value);
                productions.Add(new ProductionGroup(left, rights));
            }
            XElement XRandomSettings = XGameSettings.Element("RandomSettings");
            int totalPossibility = int.Parse(XRandomSettings.Attribute("TotalPossibility").Value);
            List<int> possibilities = new List<int>();
            foreach (var XPossibility in XRandomSettings.Element("Possibilities").Elements())
                possibilities.Add(int.Parse(XPossibility.Value));
            RandomSettings randomSettings;
            randomSettings = new RandomSettings(totalPossibility, possibilities);
            return new GameSettings(numberOfMoves, productions, randomSettings);
        }

        public void WriteToFile(string filename)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var purchaseOrderFilepath = Path.Combine(currentDirectory, filename);
            using (StreamWriter fs = new StreamWriter(purchaseOrderFilepath))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(StreamWriter stream)
        {
            XElement XEGameSettings = new XElement("GameSettings");
            XEGameSettings.Add(new XAttribute("NumberOfMoves", NumberOfMoves));
            {// add productions
                XElement XProductions = new XElement("Productions");
                foreach (var production in productions)
                {
                    XElement Xprod = new XElement("Production");
                    Xprod.Add(new XAttribute("Left", production.Left));

                    for (int i = 0; i < production.RightSize; ++i)
                        Xprod.Add(new XElement("Right", production.getRightAt(i)));
                    XProductions.Add(Xprod);
                }
                XEGameSettings.Add(XProductions);
            }
            {//add random settings
                XElement XRandomSettings = new XElement("RandomSettings");
                XRandomSettings.Add(new XAttribute("TotalPossibility", RandomSettings.getTotalPossibility()));
                XElement XPossibilities = new XElement("Possibilities");
                for (int i = 0; i < ProductionsCount; ++i)
                    XPossibilities.Add(new XElement("Possibility", RandomSettings.getProductionPossibility(i)));
                XRandomSettings.Add(XPossibilities);
                XEGameSettings.Add(XRandomSettings);
            }
            {//write
                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
                {
                    XEGameSettings.WriteTo(xmlWriter);
                }
            }
        }

        /// <summary>
        /// format :
        /// [numberOfMoves]
        /// [numberOfProdustion]
        /// produstions in next [numberOFProdution] lines 1 production in one line
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(NumberOfMoves.ToString());
            sb.AppendLine(ProductionsCount.ToString());
            foreach (ProductionGroup pr in productions)
            {
                sb.AppendLine(pr.ToString());
            }
            return sb.ToString();
        }
    }
}
