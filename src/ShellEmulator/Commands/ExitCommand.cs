using System.IO;

namespace ShellEmulator.Commands
{
    /// <summary>Команда штатного завершения эмулятора.</summary>
    public class ExitCommand : ICommand
    {
        /// <summary>Разрешает выход только при отсутствии аргументов.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            if (arguments.Length != 0)
            {
                error.WriteLine("Ошибка: exit не принимает аргументы. Использование: exit");
                return CommandResult.Error;
            }

            return CommandResult.Exit;
        }
    }
}
