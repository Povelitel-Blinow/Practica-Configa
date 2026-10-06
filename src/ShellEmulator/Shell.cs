using System;
using System.IO;
using System.Text.Json;

namespace ShellEmulator
{
    /// <summary>Интерактивный прототип оболочки варианта 9, этап 1.</summary>
    public class Shell
    {
        private const int CommandIndex = 0;
        private const int FirstArgumentIndex = 1;
        private const int MaximumCdArguments = 1;
        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly TextWriter _error;

        /// <summary>Сохраняет потоки ввода, вывода и ошибок.</summary>
        public Shell(TextReader input, TextWriter output, TextWriter error)
        {
            _input = input;
            _output = output;
            _error = error;
        }

        /// <summary>Строит приглашение по реальным данным ОС.</summary>
        public static string GetPrompt()
        {
            return Environment.UserName + "@" + Environment.MachineName + ":~$ ";
        }

        /// <summary>Читает команды до успешного exit или конца входного потока.</summary>
        public void Run()
        {
            while (true)
            {
                _output.Write(GetPrompt());
                _output.Flush();
                string line = _input.ReadLine();
                if (line == null)
                {
                    return;
                }

                if (!Execute(line))
                {
                    return;
                }
            }
        }

        /// <summary>Исполняет строку; возвращает false только при корректном exit.</summary>
        public bool Execute(string line)
        {
            try
            {
                string[] words = CommandParser.Parse(line);
                if (words.Length == 0)
                {
                    return true;
                }

                string[] arguments = new string[words.Length - FirstArgumentIndex];
                for (int index = FirstArgumentIndex; index < words.Length; index++)
                {
                    arguments[index - FirstArgumentIndex] = words[index];
                }

                return Dispatch(words[CommandIndex], arguments);
            }
            catch (FormatException exception)
            {
                _error.WriteLine("Ошибка: " + exception.Message);
                return true;
            }
        }

        /// <summary>Выбирает встроенную команду либо сообщает о неизвестной команде.</summary>
        private bool Dispatch(string command, string[] arguments)
        {
            switch (command)
            {
                case "ls":
                    PrintStub(command, arguments);
                    return true;
                case "cd":
                    ExecuteCd(arguments);
                    return true;
                case "exit":
                    return ExecuteExit(arguments);
                default:
                    _error.WriteLine("Ошибка: неизвестная команда: " + command);
                    return true;
            }
        }

        /// <summary>Проверяет число аргументов cd и выводит заглушку.</summary>
        private void ExecuteCd(string[] arguments)
        {
            if (arguments.Length > MaximumCdArguments)
            {
                _error.WriteLine("Ошибка: cd принимает не более одного аргумента. Использование: cd [путь]");
                return;
            }

            PrintStub("cd", arguments);
        }

        /// <summary>Разрешает завершение оболочки только для exit без аргументов.</summary>
        private bool ExecuteExit(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                return false;
            }

            _error.WriteLine("Ошибка: exit не принимает аргументы. Использование: exit");
            return true;
        }

        /// <summary>Выводит имя команды и аргументы, не обращаясь к файловой системе.</summary>
        private void PrintStub(string command, string[] arguments)
        {
            _output.WriteLine(command + ": " + JsonSerializer.Serialize(arguments));
        }
    }
}
