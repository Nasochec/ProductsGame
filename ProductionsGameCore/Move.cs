using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    public class Move
    {
        private List<PrimaryMove> moves;
        public int WordNumber { get; private set; }

        //private Queue<KeyValuePair<int, int>> moves = new Queue<KeyValuePair<int, int>>();

        public Move()
        {
            moves = new List<PrimaryMove>();
        }

        public int MovesNumber { get { return moves.Count; } }

        public void addMove(int wordNumber, int groupNumber, int productionNumber)
        {
            moves.Add(new PrimaryMove(wordNumber,groupNumber, productionNumber));
        }

        public IEnumerable<PrimaryMove> getMoves() { 
            return moves.AsEnumerable();
        }

        //public void popMove(out int groupNumber, out int productionNumber)
        //{
        //    KeyValuePair<int, int> move = moves.Dequeue();
        //    groupNumber = move.Key;
        //    productionNumber = move.Value;
        //}

        public static Move FromString(string move) {
            Move moveRez = new Move();
            string[] splittedMove = move.Split('\n');
            for (int i = 0; i < splittedMove.Length; i++) {
                string[] sss = splittedMove[i].Split(' ');
                int wordNumber, productionGroupNumber, productionNumber;
                if (!int.TryParse(sss[0], out wordNumber)
                    || !int.TryParse(sss[1], out productionGroupNumber)
                    || !int.TryParse(sss[2], out productionNumber))
                    throw new ArgumentException("String was in wrong format.");
                moveRez.addMove(wordNumber, productionGroupNumber, productionNumber);
            }
            return moveRez;
        }
        
        public override string ToString() { 
            StringBuilder sb = new StringBuilder();
            foreach (PrimaryMove move in this.moves) { 
                sb.AppendLine(move.ToString());
            }
            return sb.ToString();
        }
    }
}
