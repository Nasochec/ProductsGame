using ProductionsGame;
using ProductionsGameCore;
using StrategyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if false
namespace Strategies
{
    public class AnotherSearchStrategy : Strategy
    {
        int maxDepth = 4;
        List<SimplifiedWord> simpleWords = new List<SimplifiedWord>();

        double[] netMetric;
        double[][] prodsMetric;

        List<SimplifiedProductionGroup> simplifiedProds = new List<SimplifiedProductionGroup>();
        List<double> wordsMetric = new List<double>();

        List<KeyValuePair<int, int>> redux = new List<KeyValuePair<int, int>>();// productions of type A->ab Nonterminal->Terminals
        List<KeyValuePair<int, int>> recursion = new List<KeyValuePair<int, int>>();//productions of type A->*A*
        List<KeyValuePair<int, int>> other = new List<KeyValuePair<int, int>>();

        
        /// <summary>
        /// Gets parameter depth - septh of recursion in search.
        /// </summary>
        /// <param name="parameters"></param>
        public AnotherSearchStrategy(Parameters parameters) : base("Another Search Strategy")
        {
            var param = parameters.getParameter("depth");
            if (param != null && param.Value >= 0)
                maxDepth = param.Value;
        }

        static bool eq(SimplifiedWord w1, SimplifiedWord w2) {
            var nonterminals = w1.getNonterminals();
            foreach (var nonterminal in nonterminals) { 
                if(w2.getNonterminal(nonterminal.Key)!= nonterminal.Value)
                    return false;
            }
            return true;
        }

        static bool beq(SimplifiedWord w1, SimplifiedWord w2) {
            var nonterminals = w1.getNonterminals();
            foreach (var nonterminal in nonterminals)
            {
                if (nonterminal.Value < w2.getNonterminal(nonterminal.Key))
                    return false;
            }
            return true;
        }

        public class Pair<T, U>
        {
            public Pair()
            {
            }

            public Pair(T first, U second)
            {
                this.First = first;
                this.Second = second;
            }

            public T First { get; set; }
            public U Second { get; set; }
        };

        void dfs(List<SimplifiedWord> trail, List<Pair<int, int>> prodTrail) {
            var w = new SimplifiedWord(trail.Last());
            if (trail.Count > 1)
            {
                if (eq(w, trail.First()))
                {
                    Console.WriteLine(prodTrail.Count);
                }
                for (int i = 1; i < trail.Count; ++i)
                    if (beq(w, trail[i]))
                        return;
            }

            prodTrail.Add(new Pair<int, int>(0, 0));
            trail.Add(w);
            for(int i=0;i<simplifiedProds.Count;++i) {
                if (trail.Last().getNonterminal(simplifiedProds[i].Left) < 1)//checked can be applied
                    continue;
                
                w.addNonterminal(simplifiedProds[i].Left, -1);
                prodTrail.Last().First = i;

                for (int j = 0; j < simplifiedProds[i].rights.Count; ++j) {
                    prodTrail.Last().Second = j;
                    w.terminals += simplifiedProds[i].rights[j].terminals; 
                    foreach (var nonterminal in simplifiedProds[i].rights[j].getNonterminals())
                        w.addNonterminal(nonterminal.Key, nonterminal.Value);

                    dfs(trail,prodTrail);

                    w.terminals -= simplifiedProds[i].rights[j].terminals;
                    foreach (var nonterminal in simplifiedProds[i].rights[j].getNonterminals())
                        w.addNonterminal(nonterminal.Key, -nonterminal.Value);
                }
                w.addNonterminal(simplifiedProds[i].Left, +1);
            }
            prodTrail.RemoveAt(prodTrail.Count - 1);
            trail.RemoveAt(trail.Count - 1);
        }

        protected void findCycles() { 
            /*
             * Ищем новые состояния, сравничаем по ко-ву нетерминалов, если текущее положение рвно или больше по всем нетерминалам (кроме начального) прерываем рассмотрение, откатываемся назад по дереву отхода
             * Почем? - потому что нам необходимо для нахождения цикла придти к начальному терминалу, значит уже имеющиеся нетермналы надо сократить, а получая ещё больше таких нетерминалов, мы не будем приближаться, а лишь дальше уходить
             * 
             */
        }

        protected override void beforeStart()
        {

            //get the simplified form of productions

            for (int i = 0; i < GameSettings.ProductionsCount; ++i)
                simplifiedProds.Add(new SimplifiedProductionGroup(GameSettings.getProductionGroup(i)));

            //count metric of broductions

            StrategyUtilitiesClass.countMetric(simplifiedProds, GameSettings, out netMetric, out prodsMetric);
        }

        public override Move makeMove(int productionNumber, int MoveNumber, List<List<string>> words, Bank bank)
        {
            //get simplified form of words
            simpleWords.Clear();
            foreach (var word in words[PlayerNumber])
            {
                simpleWords.Add(new SimplifiedWord(word));
            }

            Move move = new Move();

            //count metric of all words
            wordsMetric.Clear();
            for (int i = 0; i < simpleWords.Count; ++i)
                wordsMetric.Add(StrategyUtilitiesClass.countWordMetric(simpleWords[i], GameSettings.RandomSettings, netMetric, simplifiedProds));

            Move mov = findFirstMove(simpleWords, wordsMetric, bank, productionNumber);
            if (mov != null && mov.MovesCount != 0)
                move.addMove(mov);
            else
            {
                return move;
            }

            while (true)
            {
                Move move1 = findMove(simpleWords, wordsMetric, bank);
                if (move1 != null && move1.MovesCount != 0)
                {
                    move.addMove(move1);
                }
                else
                    break;
            }
            return move;
        }

        void searchMove(SimplifiedWord oldWord,
            int wordIndex,
            Bank bank,
            out string bestMove,
            out double bestMetric,
            out double bestTerminals,
            Move currentMove = null,
            int depth = 0)
        {

            bestMove = "";
            bestMetric = -1;
            bestTerminals = 0;
            bool found = false;
            if (depth < maxDepth)
            {
                if (currentMove == null)
                    currentMove = new Move();
                string bMove;
                double bMetric, bTerminals;
                SimplifiedProductionGroup prod;
                for (int prodIndex = 0; prodIndex < simplifiedProds.Count; ++prodIndex)
                {
                    if (bank.getProductionCount(prodIndex) <= 0) continue;

                    prod = simplifiedProds[prodIndex];
                    if (oldWord.getNonterminal(prod.Left) <= 0) continue;
                    found = true;
                    bank.removeProduction(prodIndex);
                    oldWord.addNonterminal(prod.Left, -1);
                    for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
                    {

                        oldWord.terminals += prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            oldWord.addNeterminal(neterminal.Key, neterminal.Value);

                        currentMove.addMove(wordIndex, prodIndex, rightIndex);
                        searchMove(oldWord, wordIndex, bank,
                            out bMove, out bMetric, out bTerminals, currentMove, depth + 1);
                        if (bestMetric == -1 || bMetric > bestMetric || bMetric == bestMetric && bTerminals > bestTerminals)
                        {
                            bestMetric = bMetric;
                            bestMove = bMove;
                            bestTerminals = bTerminals;
                        }
                        currentMove.popMove();

                        oldWord.terminals -= prod.rights[rightIndex].terminals;
                        foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                            oldWord.addNeterminal(neterminal.Key, -neterminal.Value);
                    }
                    oldWord.addNonterminal(prod.Left, 1);
                    bank.addProduction(prodIndex);
                }
            }
            if (!found)
            {
                bestMetric = StrategyUtilitiesClass.countWordMetric(oldWord, rs, netMetric, simplifiedProds);
                bestMove = currentMove.ToString();
                bestTerminals = oldWord.terminals;
            }
        }

        Move findFirstMove(List<SimplifiedWord> simpleWords,
            List<double> wordsMetric, Bank bank, int productionGroupNumber
            )
        {

            var prod = simplifiedProds[productionGroupNumber];
            //fond allowed words
            List<int> allowedWords = StrategyUtilitiesClass.findMatches(simpleWords, prod.Left).ToList();

            int maxIndex;
            SimplifiedWord word = null;
            {
                double maxMetric = -1;
                maxIndex = 0;
                //find word with minimum metric
                foreach (var index in allowedWords)
                {
                    if (wordsMetric[index] > maxMetric)
                    {
                        maxMetric = wordsMetric[index];
                        maxIndex = index;
                        word = simpleWords[index];
                    }
                }
                //if can create new word
                if (prod.Left == 'S' &&
                    maxMetric < StrategyUtilitiesClass.countWordMetric(new SimplifiedWord("S"), rs, netMetric, simplifiedProds))
                {
                    maxIndex = -1;
                    word = new SimplifiedWord("S");
                }
                else if (maxMetric == -1)
                    return null;
            }
            Move move = new Move();
            string bestMove = "", tmpMove;
            double bestMetric = -1, tmpMetric, bestTerminals = 0, tmpTerminals;
            //if we can create new word (production S->) 
            int newIndex = (maxIndex == -1 ? simpleWords.Count : maxIndex);
            //senumerate all productions what we can apply to the word
            word.addNonterminal(prod.Left, -1);
            for (int rightIndex = 0; rightIndex < prod.RightSize; ++rightIndex)
            {
                word.terminals += prod.rights[rightIndex].terminals;
                foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                    word.addNeterminal(neterminal.Key, neterminal.Value);

                move.addMove(maxIndex, productionGroupNumber, rightIndex);
                searchMove(word,
                    newIndex,
                    bank,
                    out tmpMove,
                    out tmpMetric,
                    out tmpTerminals,
                    move);
                if (bestMetric == -1 || tmpMetric > bestMetric
                    || tmpMetric == bestMetric && tmpTerminals > bestTerminals)
                {
                    bestMetric = tmpMetric;
                    bestMove = tmpMove;
                    bestTerminals = tmpTerminals;
                }
                move.popMove();

                word.terminals -= prod.rights[rightIndex].terminals;
                foreach (var neterminal in prod.rights[rightIndex].neterminalsCount)
                    word.addNeterminal(neterminal.Key, -neterminal.Value);
            }
            word.addNonterminal(prod.Left, 1);

            move = Move.FromString(bestMove);
            bank.addProduction(productionGroupNumber);
            StrategyUtilitiesClass.applyMove(move, bank, word, simplifiedProds);
            return move;
        }

        Move findMove(List<SimplifiedWord> simpleWords,
            List<double> wordsMetric,
            Bank bank
            )
        {
            string bestMove;
            double bestMetric, bestTerminals;
            List<int> allowedIndexes = new List<int>();
            for (int i = 0; i < simpleWords.Count; i++)
            {
                for (int prodIndex = 0; prodIndex < simplifiedProds.Count; ++prodIndex)
                {
                    var prod = simplifiedProds[prodIndex];
                    if (simpleWords[i].getNonterminal(prod.Left) > 0 && bank.getProductionCount(prodIndex) > 0)
                        allowedIndexes.Add(i);
                }
            }
            if (allowedIndexes.Count == 0)//no words found
                return null;
            int wordNumber = -1;
            //select word with best metric
            {
                double maxMetric = -1;
                for (int i = 0; i < allowedIndexes.Count; ++i)
                {
                    var word = simpleWords[allowedIndexes[i]];
                    double metric = StrategyUtilitiesClass.countWordMetric(word,
                        rs,
                        netMetric,
                        simplifiedProds);
                    if (metric > maxMetric && metric != 1)
                    {
                        maxMetric = metric;
                        wordNumber = i;
                    }
                }
                wordNumber = allowedIndexes[wordNumber];
            }
            //find best move
            searchMove(simpleWords[wordNumber],
                wordNumber,
                bank,
                out bestMove,
                out bestMetric,
                out bestTerminals);
            Move move1 = Move.FromString(bestMove);
            wordsMetric[wordNumber] = bestMetric;
            StrategyUtilitiesClass.applyMove(move1, bank, simpleWords[wordNumber], simplifiedProds);
            return move1;
        }
    }
}
#endif