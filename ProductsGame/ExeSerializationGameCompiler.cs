﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductionsGameCore;
using ProductsGameLauncher;

namespace ProductsGame
{
    public class ExeSerializationGameCompiler : GameCompiler
    {
       
        public ExeSerializationGameCompiler(GameSettings gameSettings, IEnumerable<string> playersFilenames)
            : base(gameSettings)
        {
            if (playersFilenames.Count() != gameSettings.NumberOfPlayers)
                throw new ArgumentException("Количество игроков должно быть равно количеству игроков указанному в конфигурации игры.");
            var playerFilenameEnumerator = playersFilenames.GetEnumerator();
            for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
            {
                playerFilenameEnumerator.MoveNext();
                players.Add(new ExeSerializationPlayerAdapter(playerNumber, this, log, playerFilenameEnumerator.Current));
            }
            log.WriteLine();
            int index = 0;
            foreach (var player in playersFilenames)
            {
                index++;
                log.WriteLine("Player " + index + ": " + player);
            }
        }
    }
}
