using System.IO;
using ShellEmulator.Vfs;

namespace ShellEmulator.Commands
{
    /// <summary>Меняет рабочий каталог VFS.</summary>
    public class CdCommand : ICommand
    {
        private const int MaximumArguments = 1;
        private readonly VfsSession _session;

        /// <summary>Принимает общую сессию оболочки.</summary>
        public CdCommand(VfsSession session)
        {
            _session = session;
        }

        /// <summary>Переходит по пути; без аргументов возвращается в корень.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            if (arguments.Length > MaximumArguments)
            {
                error.WriteLine("Ошибка: cd принимает не более одного аргумента. Использование: cd [путь]");
                return CommandResult.Error;
            }

            _session.ChangeDirectory(arguments.Length == 0 ? "/" : arguments[0]);
            return CommandResult.Success;
        }
    }
}
