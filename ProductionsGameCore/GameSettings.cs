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
    [Serializable]
    public class GameSettings : ISerializable
    {
        public List<ProductionGroup> productions = new List<ProductionGroup>();
        public RandomSettings RandomSettings { get; private set; }
        public bool IsBankShare { get; private set; }
        public int NumberOfPlayers { get; private set; }
        public int NumberOfMoves { get; private set; }
        public int ProductionsCount { get { return productions.Count; } }

        public GameSettings(bool isBankShare,
            int numberOfPlayers,
            int numberOfMoves,
            IEnumerable<ProductionGroup> productions,
            RandomSettings randomSettings)
        {
            IsBankShare = isBankShare;
            if (numberOfMoves <= 0)
                throw new ArgumentException("Количествол ходов должно быть неотрицательным числом.");
            NumberOfMoves = numberOfMoves;
            if (numberOfPlayers <= 0)
                throw new ArgumentException("Количествол игороков должно быть неотрицательным числом.");
            NumberOfPlayers = numberOfPlayers;
            this.productions = productions.ToList();
            RandomSettings = randomSettings;
        }

        public GameSettings(SerializationInfo info, StreamingContext context)
        {
            IsBankShare = (bool)info.GetValue("isBankShare", typeof(bool));
            NumberOfMoves = (int)info.GetValue("numberOfMoves", typeof(int));
            NumberOfPlayers = (int)info.GetValue("numberOfPlayers", typeof(int));
            productions = (List<ProductionGroup>)info.GetValue("productions", typeof(List<ProductionGroup>));
            RandomSettings = (RandomSettings)info.GetValue("randomSettings", typeof(RandomSettings));
        }


        public ProductionGroup getProductionGroup(int index)
        {
            if (index >= 0 && index < ProductionsCount)
                return productions[index];
            throw new IndexOutOfRangeException(
                String.Format("Индекс {0} был вне границ [0,{1}).", index, ProductionsCount)
                );
        }

        public IEnumerable<ProductionGroup> GetProductions()
        {
            return productions.AsEnumerable();
        }

        public static GameSettings ReadFromFile(string filename)
        {//TODO Проверить что файл существует
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return ReadFromStream(fs);
            }
        }

        public static GameSettings ReadFromStream(Stream s)
        {
            XElement XGameSettings = null;
            try
            {
                XGameSettings = XElement.Load(s);
            }
            catch
            {
                throw new IOException("Входной поток в неверном формате: неудалось считать xml данные.");
            }
            //Считываем простые свойства
            bool isBankShare = XGameSettings.Attribute("IsBankShare").Value.Equals("true");
            int numberOfPlayers = int.Parse(XGameSettings.Attribute("NumberOfPlayers").Value);
            int numberOfMoves = int.Parse(XGameSettings.Attribute("NumberOfMoves").Value);

            //считываем иформацию о группах продукций
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
            //Считываем иформацию о вероятностях групп продукций
            XElement XRandomSettings = XGameSettings.Element("RandomSettings");
            int totalPossibility = int.Parse(XRandomSettings.Attribute("TotalPossibility").Value);
            int? seed = null;
            if (XRandomSettings.Attribute("Seed") != null)
                seed = int.Parse(XRandomSettings.Attribute("Seed").Value);
            List<int> possibilities = new List<int>();
            foreach (var XPossibility in XRandomSettings.Element("Possibilities").Elements())
                possibilities.Add(int.Parse(XPossibility.Value));
            RandomSettings randomSettings;
            if (seed != null)
                randomSettings = new RandomSettings(totalPossibility, possibilities, seed.Value);
            else
                randomSettings = new RandomSettings(totalPossibility, possibilities);
            return new GameSettings(isBankShare, numberOfPlayers, numberOfMoves, productions, randomSettings);
        }

        public void WriteToFile(string filename, bool saveSeed = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var purchaseOrderFilepath = Path.Combine(currentDirectory, filename);
            using (StreamWriter fs = new StreamWriter(purchaseOrderFilepath))
            {
                WriteToStream(fs, saveSeed);
            }
        }

        public void WriteToStream(StreamWriter stream, bool saveSeed = true)
        {

            XElement XEGameSettings = new XElement("GameSettings");
            XEGameSettings.Add(new XAttribute("IsBankShare", IsBankShare));
            XEGameSettings.Add(new XAttribute("NumberOfPlayers", NumberOfPlayers));
            XEGameSettings.Add(new XAttribute("NumberOfMoves", NumberOfMoves));
            { //Добавляем данные о группах продукций
                XElement XProductions = new XElement("Productions");
                foreach (var production in productions)
                {
                    XElement Xprod = new XElement("Production");
                    Xprod.Add(new XAttribute("Left", production.Left));
                    //Xprod.Add(new XAttribute("RightSize", production.RightSize));//TODO delete?
                    for (int i = 0; i < production.RightSize; ++i)
                        Xprod.Add(new XElement("Right", production.getRightAt(i)));
                    XProductions.Add(Xprod);
                }
                XEGameSettings.Add(XProductions);
            }
            {//Добавляем данные о вероятностях групп продукций
                XElement XRandomSettings = new XElement("RandomSettings");
                if (saveSeed && RandomSettings.Seed != null)//TODO delete seed from config?
                    XRandomSettings.Add(new XAttribute("Seed", RandomSettings.Seed.Value));
                XRandomSettings.Add(new XAttribute("TotalPossibility", RandomSettings.getTotalPossibility()));
                XElement XPossibilities = new XElement("Possibilities");
                for (int i = 0; i < ProductionsCount; ++i)
                    XPossibilities.Add(new XElement("Possibility", RandomSettings.getProductionPossibility(i)));
                XRandomSettings.Add(XPossibilities);
                XEGameSettings.Add(XRandomSettings);
            }
            {
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
        /// [isBankShare]
        /// [numberOfPlayers]
        /// [numberOfMoves]
        /// [numberOfProdustion]
        /// produstions in next [numberOFProdution] lines 1 production in one line
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(IsBankShare ? "1" : "0");
            sb.AppendLine(NumberOfPlayers.ToString());
            sb.AppendLine(NumberOfMoves.ToString());
            sb.AppendLine(ProductionsCount.ToString());
            foreach (ProductionGroup pr in productions)
            {
                sb.AppendLine(pr.ToString());
            }
            return sb.ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("isBankShare", IsBankShare, typeof(bool));
            info.AddValue("numberOfMoves", NumberOfMoves, typeof(int));
            info.AddValue("numberOfPlayers", NumberOfPlayers, typeof(int));
            info.AddValue("productions", productions, typeof(List<ProductionGroup>));
            info.AddValue("randomSettings", RandomSettings, typeof(RandomSettings));
        }
    }
}
