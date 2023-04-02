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
        public int Seed { get; private set; }

        public RandomSettings(int totalPossibility, IEnumerable<int> possibilityList)
        {
            this.totalPossibility = totalPossibility;
            this.possibilityList = possibilityList.ToList();
            int sum = 0;//Проверка что введено сумма вероятностей равна знаменателю
            foreach (int possibility in possibilityList)
                sum += possibility;
            if (sum != totalPossibility)
                throw new ArgumentException("Sum of possibility list must be equal to totalPossibility.");
            //созжаём случайный сид для будуещей генерации случайных чисел, добавив некую защиту от повторений при многопоточности
            Seed = (int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId;
                //DateTime.Now.Millisecond + Thread.CurrentThread.ManagedThreadId;
        }

        public RandomSettings(int totalPossibility, IEnumerable<int> possibilityList, int seed)
        {
            this.totalPossibility = totalPossibility;
            this.possibilityList = possibilityList.ToList();
            int sum = 0;//Проверка что введено сумма вероятностей равна знаменателю
            foreach (int possibility in possibilityList)
                sum += possibility;
            if (sum != totalPossibility)
                throw new ArgumentException("Sum of possibility list must be equal to totalPossibility.");
            Seed = seed;
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

        public int getSeed()
        {
            return Seed;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("totalPossibility", totalPossibility, typeof(int));
            info.AddValue("possibilityList", possibilityList, typeof(List<int>));
        }
    }
}
