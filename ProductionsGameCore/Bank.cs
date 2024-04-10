using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    public class Bank
    {
        List<int> productionsBank;

        public Bank(int productionsNumber)
        {
            productionsBank = new List<int>(productionsNumber);
            for (int i = 0; i < productionsNumber; ++i)
                productionsBank.Add(0);
        }

        public Bank(IEnumerable<int> productions)
        {
            productionsBank = productions.ToList();
        }

        public void addProduction(int productionIndex)
        {
            addProduction(productionIndex, 1);
        }

        public void addProduction(int productionIndex, int count)
        {
            productionsBank[productionIndex] += count;
        }

        public void removeProduction(int productionIndex)
        {
            removeProduction(productionIndex, 1);
        }

        public void removeProduction(int productionIndex, int count)
        {
            if (productionsBank[productionIndex] < count)
                throw new ArgumentException("The number of profuctions of index " + productionIndex + " in bank was less than " + count + ".");
            productionsBank[productionIndex] -= count;
        }

        public IEnumerable<int> getProductions()
        {
            return productionsBank.AsEnumerable();
        }

        public int getProductionCount(int productionIndex)
        {
            return productionsBank[productionIndex];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in productionsBank)
            {
                sb.Append(item + " ");
            }
            return sb.ToString();
        }
    }
}
