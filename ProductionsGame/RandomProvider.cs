using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductionsGameCore;

namespace ProductionsGame
{
    public class RandomProvider
    {
        public RandomSettings RandomSettings { get; private set; }
        private Random random;
        public int Seed;

        public RandomProvider(RandomSettings randomSettings)
        {
            RandomSettings = randomSettings;
            //создаём случайный сид для будуещей генерации случайных чисел, добавив некую защиту от повторений при многопоточности
            Seed = (int)DateTime.Now.Ticks * Thread.CurrentThread.ManagedThreadId;
            random = new Random(Seed);
        }

        public int getRandom()
        {//получаем номер продукции с учётом их вероятностей
            int r = random.Next(RandomSettings.getTotalPossibility());
            for (int i = 0; i < RandomSettings.productionsCount(); ++i)
            {
                r -= RandomSettings.getProductionPossibility(i);
                if (r < 0) return i;
            }
            return RandomSettings.productionsCount() - 1;
        }
    }
}
