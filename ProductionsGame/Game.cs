using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProductionsGame
{
    public class Game:IDisposable
    {
        public GameSettings GameSettings { get; private set; }
        public RandomSettings RandomSettings { get { return GameSettings.RandomSettings; } }
        public IEnumerable<ProductionGroup> Productions { get { return GameSettings.GetProductions(); } }
        private List<SimplifiedProductionGroup> simplifiedProductions;
        private List<Strategy> players;
        protected RandomProvider RandomProvider { get; private set; }
        public Bank Bank { get; private set; }

        public int MoveNumber { get; private set; }
        public int ActivePlayer { get; private set; }

        public List<List<string>> words;
        public List<List<SimplifiedWord>> simplifiedWords;
        List<int> scores;
        private bool scoresOutdated;
        private Winner CurrentWinner;

        public enum State {
            Ready,
            Active,
            Finished,
            Failed
        }

        public enum Winner
        {
            First = 0,
            Second = 1,
            Draw = -1
        }

        public State state { get; private set; }
        public string lastError { get; private set; }

        private StreamWriter log;
        public string LogFilename {get; private set;}

        public Game(GameSettings gameSettings,IEnumerable<Strategy> players, string logFilename = null) { 
            GameSettings = gameSettings;
            if (players.Count() != 2)
                throw new ArgumentException("There are must be two players in game.");
            this.players = players.ToList();
            MoveNumber = 0;
            ActivePlayer = 0;
            Bank = new Bank(GameSettings.ProductionsCount);
            RandomProvider = new RandomProvider(GameSettings.RandomSettings);
            words = new List<List<string>> { new List<string>() , new List<string>() };
            simplifiedWords = new List<List<SimplifiedWord>> { new List<SimplifiedWord>(),new List<SimplifiedWord>()};
            simplifiedProductions = new List<SimplifiedProductionGroup>();
            for (int i = 0; i < GameSettings.ProductionsCount; ++i)
                simplifiedProductions.Add(new SimplifiedProductionGroup(GameSettings.getProductionGroup(i)));
            scores = new List<int> { 0, 0 };
            state = State.Ready;
            
            if (logFilename != null)
                this.LogFilename = logFilename;
            else
            {
                StringBuilder sb = new StringBuilder();
                if (!Directory.Exists(@"./logs/"))//Создаём директорию для записи туда результатов
                    Directory.CreateDirectory(@"./logs/");
                sb.Append(@"./logs/");
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-"));
                sb.Append(Thread.CurrentThread.ManagedThreadId);
                sb.Append(".txt");
                this.LogFilename = sb.ToString();
            }
            log = new StreamWriter(this.LogFilename);
            log.AutoFlush = false;
            GameSettings.WriteToStream(log);
            log.WriteLine();
            for (int i = 0; i < 2; i++)
                log.WriteLine("Игрок {0}: {1}", i + 1, this.players[i].ToString());
        }

        private List<List<string>> getWordsCopies() { 
            List<List<string>> nwords = new List<List<string>>();
            foreach(var playerW in words)
            {
                nwords.Add(new List<string>());
                playerW.ForEach(x => nwords.Last().Add(x));
            }
            return nwords;
        }

        private List<List<SimplifiedWord>> getSWordsCopies()
        {
            List<List<SimplifiedWord>> nwords = new List<List<SimplifiedWord>>();
            foreach (var playerW in simplifiedWords)
            {
                nwords.Add(new List<SimplifiedWord>());
                playerW.ForEach(x => nwords.Last().Add(new SimplifiedWord(x)));
            }
            return nwords;
        }

        private void logMove(Move move,int production) {
            if (state == State.Failed) { 
                log.WriteLine("ERROR");
                log.WriteLine(lastError);
            }
            log.WriteLine("Индекс выпавшей продукции: {0}",production);
            log.WriteLine("Номер хода:{0}, Игрок: {1}", MoveNumber+1, ActivePlayer);
            log.WriteLine("Ход:{0}", move);
            log.WriteLine("Банк: {0}", Bank);
            log.Flush();
        }

        private void logFinish()
        {
            if (scoresOutdated)
                countScores();
            log.WriteLine("Результаты:");
            log.WriteLine("Счёт игрока 0: {0}", scores[0]);
            log.WriteLine("Счёт игрока 1: {0}", scores[1]);
            log.Flush();
        }

        private bool applyMoves(Move moves,int firstProductionNumber) {
            scoresOutdated = true;
            if (moves == null)
            {
                lastError = string.Format("The {0} Player {1} finished game with error: {2}",
                    ActivePlayer, players[ActivePlayer].Name, "player error");
                state = State.Failed;
                return false;
            }
            bool isfirst = true;
            

            foreach (var move in moves.getMoves())
            {
                int productionGroupNumber = move.ProductionGroupNumber,
                    productionNumber = move.ProductionNumber,
                    wordNumber = move.WordNumber;
                ProductionGroup production = GameSettings.getProductionGroup(productionGroupNumber);
                SimplifiedProductionGroup simplifiedProduction = simplifiedProductions[productionGroupNumber];
                char left = production.Left;
                string right = production.getRightAt(productionNumber);

                if (isfirst)
                {//Проверка что указана верная продукция 
                    if (firstProductionNumber != productionGroupNumber) {
                        lastError = String.Format("Неверный ход. Номер группы продукции должен быть {0}, но был {1}.", firstProductionNumber, productionNumber);
                        state = State.Failed;
                        return false;
                    }
                }
                else
                {//Проверка что указаная продукция есть в банке
                    if (Bank.getProductionCount(productionGroupNumber) == 0)
                    {
                        lastError = String.Format("Неверный ход. В банке не содержится продукций с номером {0}.", productionGroupNumber);
                        state = State.Failed;
                        return false;
                    }
                }
                if (wordNumber >= 0 && wordNumber < words[ActivePlayer].Count)
                {
                    string word = words[ActivePlayer][wordNumber];
                    SimplifiedWord sword = simplifiedWords[ActivePlayer][wordNumber];
                    int index = word.IndexOf(left);
                    if (sword.getNonterminal(left) <= 0 || index==-1)
                    {
                        lastError = String.Format("Неверный ход. Вывод {0} не содержит нетерминала {1}", word, left);
                        state = State.Failed;
                        return false;
                    }
                    else
                    {//Применяем продукцию. B->cDe : aBf-> acDef
                        string newWord = word.Substring(0, index) +
                            right +
                            word.Substring(index + 1, word.Length - index - 1);
                        words[ActivePlayer][wordNumber] = newWord;
                        //Применяем к упрощённой форме
                        sword.addNonterminal(left, -1);
                        sword.terminals += simplifiedProduction.rights[productionNumber].terminals;
                        foreach (var nonterminal in simplifiedProduction.rights[productionNumber].nonterminals)
                            sword.addNonterminal(nonterminal.Key, nonterminal.Value);
                    }
                }
                else
                {//Применяем продукцию к новому слову
                    if (left == 'S')
                    {
                        words[ActivePlayer].Add(right);
                        simplifiedWords[ActivePlayer].Add(new SimplifiedWord(simplifiedProduction.rights[productionNumber]));
                    }
                }
                if (!isfirst)  //Удаляем продукцию из банка
                    Bank.removeProduction(productionGroupNumber);
                isfirst = false;
            }
            if (isfirst)//осталось истиной -> ни одна продукция не была применена, добавить её в банк
                Bank.addProduction(firstProductionNumber);

            {// проверить что применены все возможные продукции
                if (moves.MovesCount == 0)//если не была применена первая продукция
                {
                    char left = GameSettings.getProductionGroup(firstProductionNumber).Left;
                    if (left == 'S')
                    {
                        lastError  = String.Format("Группа продукций {0} может быть применена для создания нового вывода, но не была применена.", firstProductionNumber);
                        state = State.Failed;
                        return false;
                    }
                    foreach (var word in simplifiedWords[ActivePlayer])
                    {
                        if (word.getNonterminal(left) >0)
                        {
                            lastError = String.Format("Группа продукций {0} может быть применена к выводу {1}.", firstProductionNumber, word);
                            state = State.Failed;
                            return false;
                        }
                    }

                }
                else
                {//проверим сто в банке нет применимых прдукций
                    int productionNumber = 0;
                    foreach (var production in GameSettings.GetProductions())
                    {
                        if (Bank.getProductionCount(productionNumber) <= 0)
                            continue;
                        char left = production.Left;
                        productionNumber++;
                        foreach (var word in simplifiedWords[ActivePlayer])
                        {
                            if (word.getNonterminal(left) > 0)
                            {
                                lastError = String.Format("Группа продукций {0} может быть применена к выводу {1}.", firstProductionNumber, word);
                                state = State.Failed;
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void play()
        {
            if (state == State.Ready || state == State.Active)
            {
                state = State.Active;
                for (MoveNumber = 0; MoveNumber < GameSettings.NumberOfMoves; ++MoveNumber)
                {
                    for (ActivePlayer = 0; ActivePlayer < 2; ++ActivePlayer)
                    {
                        int production = RandomProvider.getRandom();
                        Move move = null;
                        try
                        {
                            move = players[ActivePlayer]//TODO - send copies + apply moves
                                .makeMove(ActivePlayer, MoveNumber, production, getWordsCopies(), getSWordsCopies(), new Bank(Bank.getProductions()));
                        }
                        catch (Exception e)
                        {
                            lastError = string.Format("The {0} Player {1} finished game with error: {2}",
                                ActivePlayer, players[ActivePlayer].Name, e.Message);
                            state = State.Failed;
                            logMove(move, production);
                            logFinish();
                            return;
                        }
                        bool result = applyMoves(move, production);
                        logMove(move, production);
                        if (!result)
                        {
                            logFinish();
                            return;
                        }
                    }
                }
                state = State.Finished;
                logFinish();
            }
        }

        public Move playOneMove() {
            if (state == State.Ready || state == State.Active)
            {

                //TODO chatch exception, log it
                Move move=null;
                int production= RandomProvider.getRandom();
                try
                {
                    move = players[ActivePlayer]//TODO - send copies + apply moves
                        .makeMove(ActivePlayer, MoveNumber, production, getWordsCopies(), getSWordsCopies(), new Bank(Bank.getProductions()));
                }
                catch (Exception e)
                {
                    lastError = string.Format("The {0} Player {1} finished game with error: {2}",
                        ActivePlayer, players[ActivePlayer].Name, e.Message);
                    state = State.Failed;
                    logMove(move, production);
                    logFinish();
                    return null;
                }
                bool result = applyMoves(move, production);
                logMove(move, production);
                if (!result) {
                    logFinish();
                    return null;
                }
                ++ActivePlayer;
                if (ActivePlayer >= 2)
                {
                    ++MoveNumber;
                    ActivePlayer = 0;
                }
                if (MoveNumber >= GameSettings.NumberOfMoves)
                {
                    state = State.Finished;
                    logFinish();
                }
                return move;
            }
            return null;
        }

        public List<string> getPlayers() {
            return players.Select(x=> x.ToString()).ToList();
        }

        public Winner getWinner() {
            if(scoresOutdated)
                countScores();
            return CurrentWinner;
        }

        public List<int> getScores() {
            if (scoresOutdated)
                countScores();
            return scores.ToList();
        }

        private void countScores() { 
            scoresOutdated = false;
            for(int i=0;i<2;++i) {
                scores[i]  = simplifiedWords[i].Select(word=>word.getScore()).Sum();
            }
            if (scores[0] > scores[1])
                CurrentWinner = Winner.First;
            else if (scores[0] < scores[1])
                CurrentWinner = Winner.Second;
            else
                CurrentWinner = Winner.Draw;
        }

        public void Dispose()
        {
            log.Flush();
            log.Close();
        }
    }
}
