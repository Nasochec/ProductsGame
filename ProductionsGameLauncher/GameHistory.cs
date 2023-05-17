using ProductionsGame;
using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProductionsGameLauncher
{
    /// <summary>
    /// Класс, хранящий историю ходов игроков.
    /// </summary>
    internal abstract class GameHistory
    {
        public GameSettings GameSettings { get; protected set; }
        public int currentMoveNumber { get; protected set; }
        public int currentPlayer { get; protected set; }
        protected List<string>[] playerWords { get; set; }
        protected Move currentMove;
        protected List<StrategicPrimaryMove> currentStrategicMove;
        protected Bank currentBank;
        public int currentProductionGroup { get; protected set; }

        public abstract void moveNext();

        public abstract void movePrev();

        public IEnumerable<int> getCurrentBank()
        {
            return currentBank.getProductions();
        }
        public IEnumerable<PrimaryMove> getMove()
        {
            return currentMove.getMoves();
        }
        public IEnumerable<StrategicPrimaryMove> getStrategicMove()
        {
            return currentStrategicMove;
        }

        public bool hasNextMove()
        {
            return !(currentMoveNumber >= GameSettings.NumberOfMoves ||
                    currentMoveNumber == GameSettings.NumberOfMoves - 1 &&
                    currentPlayer >= GameSettings.NumberOfPlayers - 1);
        }
        public bool hasPrevMove()
        {
            return currentMoveNumber >= 0;
        }

        public IEnumerable<string> getPlayerWords(int index)
        {
            return playerWords[index];
        }

    }

    internal class FileGameHistory : GameHistory
    {
        private List<int>[] playerProductionGroups = new List<int>[2] {
                new List<int>(),new List<int>()
            };
        private List<List<StrategicPrimaryMove>>[] playerMoves = new List<List<StrategicPrimaryMove>>[2] {
                 new List<List<StrategicPrimaryMove>>(),new List<List<StrategicPrimaryMove>>()
            };
        protected void applyMove(List<string> words, IEnumerable<StrategicPrimaryMove> moves)
        {
            foreach (var move in moves)
            {
                if (move.WordNumber < 0 || move.WordNumber >= words.Count)
                {//если слово новое
                    words.Add(move.NewWord);
                }
                else
                {
                    words[move.WordNumber] = move.NewWord;
                }
            }
        }

        public FileGameHistory(string filename)
        {
            try
            {
                string s;
                StringBuilder sb = new StringBuilder();
                StreamReader fs = new StreamReader(filename);

                while (true)
                {
                    s = fs.ReadLine();
                    sb.Append(s);
                    if (s.Equals("</GameSettings>"))
                        break;
                }

                string confString = sb.ToString();
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(confString));
                GameSettings = GameSettings.ReadFromStream(stream);

                //пропускаем строки с файлами игроков.
                fs.ReadLine();
                fs.ReadLine();
                List<string>[] currPlayersWords = new List<string>[2] {
                        new List<string>(),new List<string>()
                    };
                for (int move = 0; move < GameSettings.NumberOfMoves; ++move)
                {
                    for (int i = 0; i < GameSettings.NumberOfPlayers; ++i)
                    {
                        s = fs.ReadLine();
                        playerProductionGroups[i].Add(int.Parse(s.Split(':').Last()));
                        fs.ReadLine();
                        s = fs.ReadLine();
                        Move m = Move.FromString(s.Split(':').Last());
                        playerMoves[i].Add(StrategicPrimaryMove.fromMove(currPlayersWords[i], m.getMoves(), GameSettings.GetProductions()));
                        applyMove(currPlayersWords[i], playerMoves[i].Last());
                        s = fs.ReadLine();
                        s = fs.ReadLine();
                        s = fs.ReadLine();
                    }
                }
                fs.Close();
                currentBank = new Bank(GameSettings.ProductionsCount);
                currentMove = null;
                currentMoveNumber = -1;
                currentPlayer = -1;
                playerWords = new List<string>[2] { new List<string>(), new List<string>() };
                currentProductionGroup = -1;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Файл результатов игры в неверном формате.");
            }
        }

        public override void moveNext()
        {
            if (!hasNextMove())
                return;
            if (currentMoveNumber == -1)
            {
                currentMoveNumber = 0;
                currentPlayer = 0;
            }
            else
            {
                currentPlayer++;
                if (currentPlayer >= GameSettings.NumberOfPlayers)
                {
                    currentPlayer = 0;
                    currentMoveNumber++;
                }
            }
            currentProductionGroup = playerProductionGroups[currentPlayer][currentMoveNumber];
            currentBank.addProduction(currentProductionGroup);
            foreach (var move in playerMoves[currentPlayer][currentMoveNumber])
            {
                if (move.WordNumber < 0 || move.WordNumber >= playerWords[currentPlayer].Count)
                {//если слово новое
                    playerWords[currentPlayer].Add(move.NewWord);
                }
                else
                {
                    playerWords[currentPlayer][move.WordNumber] = move.NewWord;
                }
                currentBank.removeProduction(move.ProductionGroupNumber);
            }
            currentMove = new Move();
            StrategicPrimaryMove.toMove(playerMoves[currentPlayer][currentMoveNumber], ref currentMove);
            currentStrategicMove = playerMoves[currentPlayer][currentMoveNumber];
        }

        public override void movePrev()
        {
            if (!hasPrevMove())
                return;
            playerMoves[currentPlayer][currentMoveNumber].GetEnumerator();
            for (int i = playerMoves[currentPlayer][currentMoveNumber].Count - 1; i >= 0; --i)
            {
                var move = playerMoves[currentPlayer][currentMoveNumber][i];
                if (move.PrevWord == "")
                {
                    playerWords[currentPlayer].RemoveAt(playerWords[currentPlayer].Count - 1);
                }
                else
                {
                    playerWords[currentPlayer][move.WordNumber] = move.PrevWord;
                }
                currentBank.addProduction(move.ProductionGroupNumber);
            }
            currentBank.removeProduction(currentProductionGroup);
            currentPlayer--;
            if (currentPlayer < 0)
            {
                currentMoveNumber--;
                currentPlayer = GameSettings.NumberOfPlayers - 1;
                if (currentMoveNumber == -1)
                {
                    currentPlayer = -1;
                    currentProductionGroup = -1;
                    currentMove = null;
                    return;
                }
            }
            currentProductionGroup = playerProductionGroups[currentPlayer][currentMoveNumber];
            currentMove = new Move();
            StrategicPrimaryMove.toMove(playerMoves[currentPlayer][currentMoveNumber], ref currentMove);
            currentStrategicMove = playerMoves[currentPlayer][currentMoveNumber];
        }
    }

    internal class CompilerGameHistory : GameHistory
    {
        GameCompiler gc;
        private List<int>[] playerProductionGroups = new List<int>[2] {
                new List<int>(),new List<int>()
            };
        private List<List<StrategicPrimaryMove>>[] playerMoves = new List<List<StrategicPrimaryMove>>[2] {
                 new List<List<StrategicPrimaryMove>>(),new List<List<StrategicPrimaryMove>>()
            };


        public CompilerGameHistory(GameCompiler gc)
        {
            this.gc = gc;
            if (gc.Active || gc.Finished)
                throw new ArgumentException("Игра не должна быть уже запущенной.");
            GameSettings = gc.GetGameSettings();
            currentBank = new Bank(GameSettings.ProductionsCount);
            currentMove = null;
            currentMoveNumber = -1;
            currentPlayer = -1;
            playerWords = new List<string>[2] { new List<string>(), new List<string>() };
            currentProductionGroup = -1;
        }

        public override void moveNext()
        {
            if (!hasNextMove())
                return;
            if (currentMoveNumber == -1)
            {
                currentMoveNumber = 0;
                currentPlayer = 0;
            }
            else
            {
                currentPlayer++;
                if (currentPlayer >= GameSettings.NumberOfPlayers)
                {
                    currentPlayer = 0;
                    currentMoveNumber++;
                }
            }
            if (gc.moveNumber < currentMoveNumber ||
                gc.moveNumber == currentMoveNumber && gc.playerNumber <= currentPlayer)
            {//Если этот ход ещё не был сыгран
                int r = gc.moveNumber;
                int p = gc.playerNumber;
                Move move = gc.playOneMove();
                var bank = gc.getBank();
                playerMoves[p].Add(StrategicPrimaryMove.fromMove(playerWords[p], move.getMoves(), GameSettings.GetProductions()));
                if (move.MovesCount > 0)
                {//если шаг был совершён, то узнаем выпавшую продукцию из него
                    playerProductionGroups[p].Add(move.getMoves().First().ProductionGroupNumber);
                }
                else
                {//а если нет, то из изменения банка
                    for (int i = 0; i < GameSettings.ProductionsCount; ++i)
                        if (bank.getProductionCount(i) > currentBank.getProductionCount(i))
                            playerProductionGroups[p].Add(i);
                }
            }
            currentProductionGroup = playerProductionGroups[currentPlayer][currentMoveNumber];
            currentBank.addProduction(currentProductionGroup);
            foreach (var move in playerMoves[currentPlayer][currentMoveNumber])
            {
                if (move.WordNumber < 0 || move.WordNumber >= playerWords[currentPlayer].Count)
                {//если слово новое
                    playerWords[currentPlayer].Add(move.NewWord);
                }
                else
                {
                    playerWords[currentPlayer][move.WordNumber] = move.NewWord;
                }
                currentBank.removeProduction(move.ProductionGroupNumber);
            }
            currentMove = new Move();
            StrategicPrimaryMove.toMove(playerMoves[currentPlayer][currentMoveNumber], ref currentMove);
            currentStrategicMove = playerMoves[currentPlayer][currentMoveNumber];
        }

        public override void movePrev()
        {
            if (!hasPrevMove())
                return;
            playerMoves[currentPlayer][currentMoveNumber].GetEnumerator();
            for (int i = playerMoves[currentPlayer][currentMoveNumber].Count - 1; i >= 0; --i)
            {
                var move = playerMoves[currentPlayer][currentMoveNumber][i];
                if (move.PrevWord == "")
                {
                    playerWords[currentPlayer].RemoveAt(playerWords[currentPlayer].Count - 1);
                }
                else
                {
                    playerWords[currentPlayer][move.WordNumber] = move.PrevWord;
                }
                currentBank.addProduction(move.ProductionGroupNumber);
            }
            currentBank.removeProduction(currentProductionGroup);
            currentPlayer--;
            if (currentPlayer < 0)
            {
                currentMoveNumber--;
                currentPlayer = GameSettings.NumberOfPlayers - 1;
                if (currentMoveNumber == -1)
                {
                    currentPlayer = -1;
                    currentProductionGroup = -1;
                    currentMove = null;
                    return;
                }
            }
            currentProductionGroup = playerProductionGroups[currentPlayer][currentMoveNumber];
            currentMove = new Move();
            StrategicPrimaryMove.toMove(playerMoves[currentPlayer][currentMoveNumber], ref currentMove);
            currentStrategicMove = playerMoves[currentPlayer][currentMoveNumber];
        }
    }
}
