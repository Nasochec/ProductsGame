using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyUtilities
{
    public class StrategyUtilitiesClass
    {
        /// <summary>
        /// Ищет есть ли нетерминал в упрошённом слове.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool isHaveLetter(SimplifiedWord word, char c)
        {
            return word.neterminalsCount.ContainsKey(c) && word.neterminalsCount[c] > 0;
        }

        /// <summary>
        /// Возвращает список индексов упрощённых слов, в котрых присутствует указанный нетерминал.
        /// </summary>
        /// <param name="words"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static List<int> findMatches(IEnumerable<SimplifiedWord> words, char c)
        {
            List<int> indexes = new List<int>();
            int index = 0;
            foreach (var word in words)
            {
                if (isHaveLetter(word, c)) indexes.Add(index);
                index++;
            }
            return indexes;
        }

        /// <summary>
        /// ищет первое вхождение символа в слове. Если не найдено возвращает -1.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int isHaveLetter(string word, char c)
        {
            return word.IndexOf(c);
        }

        /// <summary>
        /// Возвращает список индексов слов, в котрых присутствует указанный символ.
        /// </summary>
        /// <param name="words"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static List<int> findMatches(IEnumerable<string> words, char c)
        {
            List<int> indexes = new List<int>();
            int index = 0;
            foreach (string word in words)
            {
                if (isHaveLetter(word, c) != -1) indexes.Add(index);
                index++;
            }
            return indexes;
        }
    }
}
