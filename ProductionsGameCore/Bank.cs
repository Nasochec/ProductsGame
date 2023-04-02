using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    [Serializable]
    public class Bank:ISerializable
    {
        List<int> productionsBank;

        public Bank(int productionsNumber)
        {
            productionsBank = new List<int>(productionsNumber);
        }

        public Bank(SerializationInfo info, StreamingContext context)
        {
            productionsBank = (List<int>)info.GetValue("productionsBank", typeof(List<int>));
        }

        public void addProduction(int productionIndex) {
            productionsBank[productionIndex]++;
        }

        public void removeProduction(int productionIndex)
        {
            productionsBank[productionIndex]--;
        }

        public IEnumerable<int> getProductions() { 
            return productionsBank.AsEnumerable();
        }

        public int getProductionCount(int productionIndex) { 
            return productionsBank[productionIndex];
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("productionsBank", productionsBank, typeof(List<int>)); 
        }
    }
}
