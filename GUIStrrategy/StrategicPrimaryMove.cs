using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIStrategy
{
    internal class StrategicPrimaryMove : PrimaryMove
    {
        public int LetterNumer { get; }
        public string PrevWord { get; }
        public string NewWord { get; }
        public StrategicPrimaryMove(int wordNumber, int letterNumber, int productionGroupNumber, int productionNumber,string prevWord, string newWord)
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
    }
}
