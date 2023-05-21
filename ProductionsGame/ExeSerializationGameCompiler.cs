using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductionsGameCore;

namespace ProductionsGame
{
    public class ExeSerializationGameCompiler : GameCompiler
    {

        public ExeSerializationGameCompiler(GameSettings gameSettings, IEnumerable<string> playersFilenames, IEnumerable<string> parameters)
            : base(gameSettings)
        {
            if (playersFilenames.Count() != gameSettings.NumberOfPlayers)
                throw new ArgumentException("Количество игроков должно быть равно количеству игроков указанному в конфигурации игры.");
            var playerFilenameEnumerator = playersFilenames.GetEnumerator();
            IEnumerator<string> parametersEnumerator = null;
            if (parameters != null)
                parametersEnumerator = parameters.GetEnumerator();
            for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
            {
                playerFilenameEnumerator.MoveNext();
                if (parametersEnumerator != null)
                {
                    parametersEnumerator.MoveNext();
                    players.Add(new ExeSerializationPlayerAdapter(playerNumber, this, log, playerFilenameEnumerator.Current,parametersEnumerator.Current));
                }
                else
                    players.Add(new ExeSerializationPlayerAdapter(playerNumber, this, log, playerFilenameEnumerator.Current));

            }
            log.WriteLine();
            int index = 0;
            foreach (var player in playersFilenames)
            {
                index++;
                log.WriteLine("Игрок " + index + ": " + player);
            }
        }

        public ExeSerializationGameCompiler(GameSettings gameSettings, IEnumerable<string> playersFilenames, IEnumerable<string> parameters, string logFilename)
            : base(gameSettings, logFilename)
        {
            if (playersFilenames.Count() != gameSettings.NumberOfPlayers)
                throw new ArgumentException("Количество игроков должно быть равно количеству игроков указанному в конфигурации игры.");
            var playerFilenameEnumerator = playersFilenames.GetEnumerator();
            IEnumerator<string> parametersEnumerator = null;
            if (parameters != null)
                parametersEnumerator = parameters.GetEnumerator();
            for (int playerNumber = 0; playerNumber < gameSettings.NumberOfPlayers; ++playerNumber)
            {
                playerFilenameEnumerator.MoveNext();
                if (parametersEnumerator != null)
                {
                    parametersEnumerator.MoveNext();
                    players.Add(new ExeSerializationPlayerAdapter(playerNumber, this, log, playerFilenameEnumerator.Current, parametersEnumerator.Current));
                }
                else
                    players.Add(new ExeSerializationPlayerAdapter(playerNumber, this, log, playerFilenameEnumerator.Current));

            }
            log.WriteLine();
            int index = 0;
            foreach (var player in playersFilenames)
            {
                index++;
                log.WriteLine("Игрок " + index + ": " + player);
            }
        }
    }
}
