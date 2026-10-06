using System;
using System.Collections.Generic;
using System.IO;
using ShellEmulator.Commands;
using ShellEmulator.Vfs;

namespace ShellEmulator
{
    /// <summary>Оболочка с общим исполнителем для REPL и стартового скрипта.</summary>
    public class Shell
    {
        private const int CommandIndex = 0;
        private const int FirstArgumentIndex = 1;
        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly TextWriter _error;
        private readonly Dictionary<string, ICommand> _commands;
        private readonly VirtualFileSystem _fileSystem;

        /// <summary>Сохраняет потоки и регистрирует встроенные команды.</summary>
        public Shell(TextReader input, TextWriter output, TextWriter error)
            : this(input, output, error, new VirtualFileSystem())
        {
        }

        /// <summary>Принимает готовую VFS в памяти, не зная о формате и расположении её источника.</summary>
        public Shell(TextReader input, TextWriter output, TextWriter error, VirtualFileSystem fileSystem)
        {
            _input = input;
            _output = output;
            _error = error;
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            _fileSystem = fileSystem;
            _commands = new Dictionary<string, ICommand>(StringComparer.Ordinal);
            RegisterCommand("ls", new LsCommand());
            RegisterCommand("cd", new CdCommand());
            RegisterCommand("exit", new ExitCommand());
        }

        /// <summary>Предоставляет файловую систему текущей сессии для будущих команд.</summary>
        public VirtualFileSystem GetFileSystem()
        {
            return _fileSystem;
        }

        /// <summary>Добавляет обработчик; повторное имя запрещено.</summary>
        public void RegisterCommand(string name, ICommand command)
        {
            if (string.IsNullOrWhiteSpace(name) || command == null)
            {
                throw new ArgumentException("Нужны имя команды и обработчик.");
            }

            _commands.Add(name, command);
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
                if (line == null || !Execute(line))
                {
                    return;
                }
            }
        }

        /// <summary>Печатает ввод скрипта и останавливается на первой ошибке или exit.</summary>
        public CommandResult RunScript(TextReader script)
        {
            string line = script.ReadLine();
            while (line != null)
            {
                _output.WriteLine(GetPrompt() + line);
                _output.Flush();
                CommandResult result = ExecuteCommand(line);
                if (result != CommandResult.Success)
                {
                    return result;
                }

                line = script.ReadLine();
            }

            return CommandResult.Success;
        }

        /// <summary>Сохраняет контракт REPL: ошибки продолжают диалог, exit завершает.</summary>
        public bool Execute(string line)
        {
            return ExecuteCommand(line) != CommandResult.Exit;
        }

        /// <summary>Разбирает строку и различает успех, ошибку и штатное завершение.</summary>
        public CommandResult ExecuteCommand(string line)
        {
            try
            {
                string[] words = CommandParser.Parse(line);
                if (words.Length == 0)
                {
                    return CommandResult.Success;
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
                return CommandResult.Error;
            }
        }

        /// <summary>Находит обработчик в реестре и выполняет его через общий контракт.</summary>
        private CommandResult Dispatch(string command, string[] arguments)
        {
            ICommand handler;
            if (!_commands.TryGetValue(command, out handler))
            {
                _error.WriteLine("Ошибка: неизвестная команда: " + command);
                return CommandResult.Error;
            }

            return handler.Execute(arguments, _output, _error);
        }
    }
}
