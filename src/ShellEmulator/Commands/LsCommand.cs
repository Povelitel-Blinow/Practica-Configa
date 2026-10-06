using System.IO;
using System.Text.Json;

namespace ShellEmulator.Commands
{
    /// <summary>Заглушка команды ls.</summary>
    public class LsCommand : ICommand
    {
        /// <summary>Выводит имя команды и все аргументы.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            output.WriteLine("ls: " + JsonSerializer.Serialize(arguments));
            return CommandResult.Success;
        }
    }
}
