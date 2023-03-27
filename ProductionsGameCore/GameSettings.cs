using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProductsGame
{
    [Serializable]
    public class GameSettings : ISerializable
    {
        List<ProductionGroup> productions = new List<ProductionGroup>();
        List<int> productionsPossibilities = new List<int>();
        public bool IsBankShare { get; private set; }
        public int NumberOfPlayers { get; private set; }
        public int NumberOfMoves { get; private set; }
        public int ProductionsCount { get { return productions.Count; } }


        public GameSettings()
        {

        }

        public GameSettings(SerializationInfo info, StreamingContext context)
        {

            IsBankShare = (bool)info.GetValue("isBankShare", typeof(bool));
            NumberOfMoves = (int)info.GetValue("numberOfMoves", typeof(int));
            NumberOfPlayers=(int)info.GetValue("numberOfPlayers",  typeof(int));
            int productionsCount= (int)info.GetValue("numberOfProductions", typeof(int));
            for (int i = 0; i < ProductionsCount; ++i)
                productions.Add((ProductionGroup)info.GetValue("production" + i,  typeof(ProductionGroup)));
            for (int i = 0; i < ProductionsCount; ++i)
                productionsPossibilities.Add((int)info.GetValue("production" + i + "Possibility", typeof(int)));            
        }


        public ProductionGroup getProductionGroup(int index)
        {
            if (index >= 0 && index < ProductionsCount)
                return productions[index];
            throw new IndexOutOfRangeException(
                String.Format("Index {0} was out of range [0,{1})", index, ProductionsCount)
                );
        }

        public IEnumerable<ProductionGroup> GetProductions()
        {
            return productions.AsEnumerable();
        }

        public static GameSettings ReadFromFile(string filename)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var purchaseOrderFilepath = Path.Combine(currentDirectory, filename);
            XElement gameSettings = XElement.Load(purchaseOrderFilepath);
            //TODO
            //gameSettings.Attribute("NummberOfPlayers").Value;
            throw new NotImplementedException();
        }

        static void WriteToFile(string filenamr)
        {
            //TODO
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(IsBankShare ? "1" : "0");
            sb.AppendLine(NumberOfPlayers.ToString());
            sb.AppendLine(NumberOfMoves.ToString());
            sb.AppendLine(ProductionsCount.ToString());
            foreach (ProductionGroup pr in productions)
            {
                sb.AppendLine(pr.ToString());
            }
            foreach (int pr in productionsPossibilities)
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
            info.AddValue("numberOfProductions", ProductionsCount, typeof(int));
            for (int i = 0; i < ProductionsCount; ++i)
                info.AddValue("production" + i, productions[i], typeof(ProductionGroup));
            for (int i = 0; i < ProductionsCount; ++i)
                info.AddValue("production" + i + "Possibility", productionsPossibilities[i], typeof(int));
        }
    }
}
