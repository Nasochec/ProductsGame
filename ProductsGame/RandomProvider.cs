using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductionsGameCore;

namespace ProductsGame
{
    public class RandomProvider
    {
        public RandomSettings RandomSettings { get; private set; }
        private Random random;

        public RandomProvider(RandomSettings randomSettings)
        {
            RandomSettings = randomSettings;
            random = new Random(randomSettings.Seed);

        }

        public int getRandom()
        {//получаем номер продукции с учётом их вероятностей
            int r = random.Next(RandomSettings.getTotalPossibility());
            for (int i = 0; i < RandomSettings.productionsCount(); ++i)
            {
                r -= random.Next(RandomSettings.getProductionPossibility(i));
                if(r<0)return i;
            }
            return RandomSettings.productionsCount()-1;
        }
    }
}
