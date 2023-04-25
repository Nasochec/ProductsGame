using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsGame
{
    public class StrategyUtilities
    {
        /// <summary>
        /// ищет первое вхождение символа в слове. Если не найдено возвращает -1.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int isHaveLetter(string word,char c) {
            return word.IndexOf(c);
        }

        public static List<int> findMatches(IEnumerable<string> words, char c) {
            List<int> indexes = new List<int>();
            int index = 0;
            foreach (string word in words) { 
                if(isHaveLetter(word,c)!=-1)indexes.Add(index);
                index++;
            }
            return indexes;
        }
        //TODO delete
        //public static List<int> findAvaliableProductions(IEnumerable<string> words, IEnumerable<ProductionGroup> productions) {
        //    List<int> indexes = new List<int>();
        //    int index = 0;
        //    foreach (var prod in productions) {
        //        if (findMatches(words, prod.Left).Count != 0 || )
        //            indexes.Add(index);
        //        index++;
        //    }
        //    return indexes;
        //}
    }
}
