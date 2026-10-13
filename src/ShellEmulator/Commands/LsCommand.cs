using System.IO;
using ShellEmulator.Vfs;

namespace ShellEmulator.Commands
{
    /// <summary>Выводит содержимое каталогов или имена файлов VFS.</summary>
    public class LsCommand : ICommand
    {
        private readonly VfsSession _session;

        /// <summary>Принимает общую сессию оболочки.</summary>
        public LsCommand(VfsSession session)
        {
            _session = session;
        }

        /// <summary>Без аргументов выводит текущий каталог; принимает несколько путей.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            string[] paths = arguments.Length == 0 ? new string[] {"."} : arguments;
            foreach (string argument in paths)
            {
                string path = _session.Resolve(argument);
                if (paths.Length > 1)
                {
                    output.WriteLine(path + ":");
                }

                PrintEntry(path, output);
            }

            return CommandResult.Success;
        }

        /// <summary>Печатает имя файла либо отсортированные имена детей каталога.</summary>
        private void PrintEntry(string path, TextWriter output)
        {
            VirtualFileSystem fileSystem = _session.GetFileSystem();
            if (!fileSystem.GetEntry(path).IsDirectory())
            {
                output.WriteLine(path.Substring(path.LastIndexOf('/') + 1));
                return;
            }

            foreach (string child in fileSystem.GetChildren(path))
            {
                output.WriteLine(child.Substring(child.LastIndexOf('/') + 1));
            }
        }
    }
}
