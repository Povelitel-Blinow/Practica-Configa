using System.IO;
using System.Text.Json;

namespace ShellEmulator.Commands
{
    /// <summary>Заглушка команды cd с проверкой числа аргументов.</summary>
    public class CdCommand : ICommand
    {
        private const int MaximumArguments = 1;

        /// <summary>Выводит путь либо сообщает о лишних аргументах.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            if (arguments.Length > MaximumArguments)
            {
                error.WriteLine("Ошибка: cd принимает не более одного аргумента. Использование: cd [путь]");
                return CommandResult.Error;
            }

            output.WriteLine("cd: " + JsonSerializer.Serialize(arguments));
            return CommandResult.Success;
        }
    }
}
