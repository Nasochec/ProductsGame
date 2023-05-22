using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    [Serializable]
    public class RandomSettings : ISerializable
    {
        private int totalPossibility;
        private List<int> possibilityList;

        public RandomSettings(int totalPossibility, IEnumerable<int> possibilityList)
        {
            this.totalPossibility = totalPossibility;
            this.possibilityList = possibilityList.ToList();
            int sum = 0;
            foreach (int possibility in possibilityList)
            {
                sum += possibility;
                if (possibility <= 0)
                    throw new ArgumentException("Probabolity must be non-negative number.");
            }
            if (sum != totalPossibility)
                throw new ArgumentException("Sum of probabilities must be equal to totalPossibility.");
        }


        public RandomSettings(SerializationInfo info, StreamingContext context)
        {
            totalPossibility = (int)info.GetValue("totalPossibility", typeof(int));
            possibilityList = (List<int>)info.GetValue("possibilityList", typeof(List<int>));
        }

        public int getProductionPossibility(int index)
        {
            return possibilityList[index];
        }

        public int productionsCount()
        {
            return possibilityList.Count;
        }

        public int getTotalPossibility()
        {
            return totalPossibility;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("totalPossibility", totalPossibility, typeof(int));
            info.AddValue("possibilityList", possibilityList, typeof(List<int>));
        }
    }
}
