using ProductionsGameCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


namespace ProductsGame
{


    /// <summary>
    /// Параметр filename конструктора отвечает за путь к приложению стратегии. Ему тна стандартный входной поток будет даваться информация для мормирования хода.
    /// Формат входных данных: в сериализованном виде класс GameSettings и номер игрока типа int, далее перед каждым шагом - номер шага типа int, текущее содержимое банка как сериализованный класс типа Bank, слова(текеущие выводы) всех игроков типа List<List<string>>, и индекс выпавшей продукции типа int.
    /// В качестве ответа должен быть передан ход в строковом формате: простейщие ходы, разделённыйе запятыми без пробелов, в конце отделённые переносом строки. Простейший ход:  индекс слова, индекс группы продукции и индекс продукции в группе продукций, разделитель - пробел.
    /// Согоашения относительно выходных кодов: 0 - успешное завершение программы после совершения всех ходов, 1 - в программе произошла ошибка исполнения, 2 - игрок сдался.
    ///
    /// </summary>
    public class ExeSerializationPlayerAdapter : PlayerAdapter
    {
        //TODO возможно надо добавить IDisposable для остановки потока стратегии
        //TODO если процесс завершён до завершения игры - ошибка
        String filename;
        private Process player;
        private BinaryFormatter formatter;
        public ExeSerializationPlayerAdapter(int number,
            GameCompiler gameCompiler,
            string logFilename,
            String filename)
            : base(number, gameCompiler, logFilename)
        {
            this.filename = filename;
            formatter = new BinaryFormatter();
            player = new Process();
            player.StartInfo.FileName = filename;
            player.StartInfo.CreateNoWindow = true;//TODO возможно надо удалить
            player.StartInfo.RedirectStandardInput = true;
            player.StartInfo.RedirectStandardOutput = true;
            player.StartInfo.UseShellExecute = false;
            //player.StartInfo.WorkingDirectory = filename;
            player.Start();
            //передаём программе необходимые данные
            formatter.Serialize(player.StandardInput.BaseStream, Settings);
            formatter.Serialize(player.StandardInput.BaseStream, MyNumber);

        }



        override protected Move CalculateMove(int productionGroupNumber)
        {

            //Получим список всех слов всех пользователей для передачи в программы стратегий.
            List<List<string>> playersWords = GameCompiler.getPlayersWords();

            //Направляем программе на вход текущую конфигурацию
            formatter.Serialize(player.StandardInput.BaseStream, MoveNumber);
            formatter.Serialize(player.StandardInput.BaseStream, Bank);
            formatter.Serialize(player.StandardInput.BaseStream, playersWords);
            formatter.Serialize(player.StandardInput.BaseStream, productionGroupNumber);

            player.StandardInput.Flush();
            string output = player.StandardOutput.ReadLine();
            if (player.HasExited)
                if (player.ExitCode == 2)
                    throw new Exception("Игрок " + MyNumber + " сдался.");
                else if (player.ExitCode != 0)
                    throw new Exception("Программа игрока " + MyNumber + " завершена с ошибкой.");
            Move move = Move.FromString(output);
            if (player.HasExited)
                if (player.ExitCode == 2)
                    throw new Exception("Игрок " + MyNumber + " сдался.");
                else if (player.ExitCode != 0)
                    throw new Exception("Программа игрока " + MyNumber + " завершена с ошибкой.");
            return move;
        }
    }
}
