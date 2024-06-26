﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    public class RandomSettings
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
    }
}
