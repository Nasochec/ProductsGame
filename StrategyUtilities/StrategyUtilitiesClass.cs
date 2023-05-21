using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyUtilities
{
    public static class StrategyUtilitiesClass
    {
        /// <summary>
        /// Ищет есть ли нетерминал в упрошённом выводе.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool isHaveLetter(SimplifiedWord word, char c)
        {
            return word.neterminalsCount.ContainsKey(c) && word.getNeterminal(c) > 0;
        }

        /// <summary>
        /// Возвращает список индексов упрощённых выводов, в котрых присутствует указанный нетерминал.
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
        /// ищет первое вхождение символа в выводе. Если не найдено возвращает -1.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int isHaveLetter(string word, char c)
        {
            return word.IndexOf(c);
        }

        /// <summary>
        /// Возвращает список индексов выводов, в котрых присутствует указанный символ.
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


        /// <summary>
        /// Находит метрику оценки продукций. 
        /// Предполагается что чем больше её значение тем за меньшее количество ходов можно получить терминальную строку.
        /// </summary>
        /// <param name="productions"></param>
        /// <param name="settings"></param>
        /// <param name="netMetric"></param>
        /// <param name="prodMetric"></param>
        public static void countMetric(List<SimplifiedProductionGroup> productions,
            GameSettings settings,
            out double[] netMetric,
            out double[][] prodMetric
            )
        {
            netMetric = new double[productions.Count];
            prodMetric = new double[productions.Count][];
            int productionsCount = productions.Count;
            const double eps = 0.00001;

            //начальная инициализация
            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
            {
                netMetric[prodIndex] = -1;
                prodMetric[prodIndex] = new double[productions[prodIndex].RightSize];
                for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                {
                    var right = productions[prodIndex].rights[rightIndex];
                    if (right.neterminalsCount.Count == 0)//терминальная строка оценивается в 1
                        netMetric[prodIndex] = prodMetric[prodIndex][rightIndex] = 1;
                    else
                        prodMetric[prodIndex][rightIndex] = -1;//ещё не посчитанная метрика обозначается -1
                }
            }
            RandomSettings rs = settings.RandomSettings;
            bool found = true;
            while (found)//остановимся когда добьёмся указанной точнсти 
            {
                found = false;
                for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
                {
                    for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                    {
                        var right = productions[prodIndex].rights[rightIndex];
                        if (right.neterminalsCount.Count != 0)
                        {//если в продукции есть нетерминалы - пересчитываем её
                            double rightSum = countWordMetric(right, rs, netMetric, productions);
                            if (Math.Abs(prodMetric[prodIndex][rightIndex] - rightSum) >= eps)
                            {
                                found = true;
                                prodMetric[prodIndex][rightIndex] = rightSum;
                                netMetric[prodIndex] = Math.Max(netMetric[prodIndex], rightSum);
                            }
                        }
                    }
                }
            }
        }

        public static double countWordMetric(SimplifiedWord word, RandomSettings rs, double[] netMetric,
            List<SimplifiedProductionGroup> productions)
        {
            int productionsCount = productions.Count;
            double rightSum = 1;
            foreach (var neterminal in word.neterminalsCount)
            {
                double sum = 0;
                for (int i = 0; i < productionsCount; ++i)
                {
                    if (netMetric[i] != -1 && productions[i].Left == neterminal.Key)
                    {
                        sum += netMetric[i] * rs.getProductionPossibility(i) / rs.getTotalPossibility();
                    }
                }
                sum = Math.Pow(sum, neterminal.Value);
                rightSum *= sum;
            }
            return rightSum;
        }
        /// <summary>
        /// Находит метрику оценки продукций. 
        /// Предполагается что чем больше её значение тем за меньшее количество ходов можно получить терминальную строку.
        /// </summary>
        /// <param name="productions"></param>
        /// <param name="settings"></param>
        /// <param name="netMetric"></param>
        /// <param name="prodMetric"></param>
        public static void countStupidMetric(List<SimplifiedProductionGroup> productions,
            out int[] netMetric,
            out int[][] prodMetric
            )
        {
            netMetric = new int[productions.Count];
            prodMetric = new int[productions.Count][];
            int productionsCount = productions.Count;

            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
                prodMetric[prodIndex] = new int[productions[prodIndex].RightSize];

            for (int prodIndex = 0; prodIndex < productionsCount; ++prodIndex)
            {
                for (int rightIndex = 0; rightIndex < productions[prodIndex].RightSize; ++rightIndex)
                {
                    var right = productions[prodIndex].rights[rightIndex];

                    int rightSum = countWordStupidMetric(right);
                    prodMetric[prodIndex][rightIndex] = rightSum;
                    netMetric[prodIndex] = Math.Min(netMetric[prodIndex], rightSum);
                }
            }

        }

        public static int countWordStupidMetric(SimplifiedWord word)
        {
            int rightSum = 0;
            foreach (var neterminal in word.neterminalsCount)
            {
                rightSum += neterminal.Value;
            }
            return rightSum;
        }
    }
}
