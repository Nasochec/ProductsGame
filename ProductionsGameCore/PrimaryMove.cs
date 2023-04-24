using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    /// <summary>
    /// Соглашение для простейших ходов: простейший ход это 3 числа: номер слова, номер группы продукций, и номер продукции в группе.
    /// Если слова с заданным номером нет, то это значит что должно добавиться новое слово, но только в том случае, если группа продукций в правой части имеет нетерминал S.
    /// </summary>
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
