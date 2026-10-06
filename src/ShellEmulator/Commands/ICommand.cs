using System.IO;

namespace ShellEmulator.Commands
{
    /// <summary>Общий контракт команд эмулятора.</summary>
    public interface ICommand
    {
        /// <summary>Проверяет аргументы и выполняет команду через заданные потоки.</summary>
        CommandResult Execute(string[] arguments, TextWriter output, TextWriter error);
    }
}
