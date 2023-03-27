using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProductsGame
{
    [Serializable]
    public class Bank:ISerializable
    {
        //List<>
        private Dictionary<ProductionGroup,int> productionsBank;

        public Bank(List<ProductionGroup> productions) {
            productionsBank = new Dictionary<ProductionGroup,int>();
            for(int i=0;i< productions.Count; i++) 
                productionsBank.Add(productions[i],0);
        }

        public Bank(IEnumerable<ProductionGroup> productions)
        {
            productionsBank = new Dictionary<ProductionGroup, int>();
            foreach (var production in productions)
                productionsBank.Add(production, 0);
        }

        public void addProduction(ProductionGroup production) {
            productionsBank[production]++;
        }

        public void removeProduction(ProductionGroup production)
        {
            productionsBank[production]--;
        }

        public IEnumerable<KeyValuePair<ProductionGroup, int>> getProductions() { 
            return productionsBank.AsEnumerable();
        }

        public int getProductionCount(ProductionGroup production) { 
            return productionsBank[production];
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("size",productionsBank.Count,typeof(int));
            for (int i = 0; i < productionsBank.Count; ++i)
                info.AddValue("production" + i, productions[i], typeof(ProductionGroup));
           
        }
    }
}
