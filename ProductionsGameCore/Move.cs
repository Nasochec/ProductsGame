using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    /// <summary>
    /// Соглашение для ходов: ход это множество простейших ходов (если оно пустое, то считается что делается пропуск хода).
    /// Соглашение для простейших ходов: простейший ход это 3 числа: номер слова, номер группы продукций, и номер продукции в группе.
    /// Если слова с заданным номером нет, то это значит что должно добавиться новое слово, но только в том случае, если группа продукций в правой части имеет нетерминал S.
    /// </summary>
    public class Move
    {
        private List<PrimaryMove> moves;
        public Move()
        {
            moves = new List<PrimaryMove>();
        }

        public int MovesCount { get { return moves.Count; } }

        public void addMove(int wordNumber, int groupNumber, int productionNumber)
        {
            moves.Add(new PrimaryMove(wordNumber, groupNumber, productionNumber));
        }

        public void popMove() {
            moves.RemoveAt(moves.Count - 1);
        }

        public IEnumerable<PrimaryMove> getMoves()
        {
            return moves.AsEnumerable();
        }

        public static Move FromString(string move)
        {
            if(move == null || move == "")
                return new Move();
            Move moveRez = new Move();
            string[] splittedMove = move.Split(',');
            for (int i = 0; i < splittedMove.Length; i++)
            {
                string[] sss = splittedMove[i].Split(' ');
                int wordNumber, productionGroupNumber, productionNumber;
                if (!int.TryParse(sss[0], out wordNumber)
                    || !int.TryParse(sss[1], out productionGroupNumber)
                    || !int.TryParse(sss[2], out productionNumber))
                    throw new ArgumentException("Входная строка в неверном формате.");
                moveRez.addMove(wordNumber, productionGroupNumber, productionNumber);
            }
            return moveRez;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (moves.Count != 0)
                sb.Append(moves[0].ToString());
            for (int index = 1; index < moves.Count; ++index)
            {
                sb.Append("," + moves[index].ToString());
            }
            return sb.ToString();
        }
    }
}
