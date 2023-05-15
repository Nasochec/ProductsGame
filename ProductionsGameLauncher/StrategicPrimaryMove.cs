using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameLauncher
{
    internal class StrategicPrimaryMove : PrimaryMove
    {
        public int LetterNumer { get; }
        public string PrevWord { get; }
        public string NewWord { get; }
        public StrategicPrimaryMove(int wordNumber, int letterNumber, int productionGroupNumber, int productionNumber, string prevWord, string newWord)
            : base(wordNumber, productionGroupNumber, productionNumber)
        {
            this.LetterNumer = letterNumber;
            this.PrevWord = prevWord;
            NewWord = newWord;
        }

        public static void toMove(IEnumerable<StrategicPrimaryMove> moves, ref Move move)
        {
            foreach (var m in moves)
            {
                move.addMove(m.WordNumber, m.ProductionGroupNumber, m.ProductionNumber);
            }
        }

        /// <summary>
        /// Из обычного хода и слов получает ход, с указанием индекса применения продукции.
        /// </summary>
        public static List<StrategicPrimaryMove> fromMove(IEnumerable<string> words, IEnumerable<PrimaryMove> move, IEnumerable<ProductionGroup> productions)
        {
            List<string> wordsL = words.ToList();
            List<StrategicPrimaryMove> mv = new List<StrategicPrimaryMove>();
            List<ProductionGroup> prods = productions.ToList();
            foreach (var m in move)
            {
                int pg = m.ProductionGroupNumber;
                int p = m.ProductionNumber;
                if (m.WordNumber < 0 || m.WordNumber >= wordsL.Count)
                {
                    mv.Add(new StrategicPrimaryMove(m.WordNumber, 0, pg, p, "", prods[pg].getRightAt(p)));
                    wordsL.Add(prods[m.ProductionGroupNumber].getRightAt(m.ProductionNumber));
                }
                else
                {
                    string oldWord = wordsL[m.WordNumber];
                    int letterIndex = StrategyUtilitiesClass.isHaveLetter(oldWord, prods[pg].Left);
                    string newWord = oldWord.Substring(0, letterIndex) +
                                prods[pg].getRightAt(p) +
                                oldWord.Substring(letterIndex + 1, oldWord.Length - letterIndex - 1);
                    mv.Add(new StrategicPrimaryMove(m.WordNumber, letterIndex, pg, p, oldWord, newWord));
                    wordsL[m.WordNumber] = newWord;
                }
            }
            return mv;
        }
    }
}
