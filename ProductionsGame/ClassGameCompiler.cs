using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGame
{
    public class ClassGameCompiler : GameCompiler
    {
        //public ClassGameCompiler(GameSettings gameSettings, IEnumerable<ClassPlayerAdapter> players, string logFilename = null)
        //    : base(gameSettings, logFilename)
        //{
        //    if (players.Count() != gameSettings.NumberOfPlayers)
        //        throw new ArgumentException("Количество игроков должно быть равно количеству игроков указанному в конфигурации игры.");
        //    foreach (var player in players)
        //        this.players.Add(player);
        //    log.WriteLine();
        //    int index = 0;
        //    foreach (var player in players)
        //    {
        //        index++;
        //        log.WriteLine("Игрок " + index + ": " + player.Name);
        //    }
        //}

        public ClassGameCompiler(GameSettings gameSettings, IEnumerable<Strategy> strategies, string logFilename = null)
            : base(gameSettings, logFilename)
        {
            if (strategies.Count() != gameSettings.NumberOfPlayers)
                throw new ArgumentException("Количество игроков должно быть равно количеству игроков указанному в конфигурации игры.");
            int number = 0;
            foreach (var strategy in strategies)
            {
                var player = new ClassPlayerAdapter(number++, this, log, strategy);
                this.players.Add(player);
            }
            log.WriteLine();
            int index = 0;
            foreach (var strategy in strategies)
            {
                index++;
                log.WriteLine("Игрок " + index + ": " + strategy.Name);
            }
        }
    }
}
