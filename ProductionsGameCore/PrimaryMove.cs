using System;

namespace ProductionsGameCore
{
    public class PrimaryMove
    {
        public int WordNumber { get; }
        public int ProductionGroupNumber { get; }
        public int ProductionNumber { get; }
        
        public PrimaryMove(int wordNumber, int productionGroupNumber, int productionNumber)
        {
            WordNumber = wordNumber;
            ProductionGroupNumber = productionGroupNumber;
            ProductionNumber = productionNumber;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}",
                WordNumber.ToString(),
                ProductionGroupNumber.ToString(),
                ProductionNumber.ToString());
        }
    }
}
