using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ShellEmulator.Vfs;

namespace ShellEmulator.Commands
{
    /// <summary>Обходит дерево VFS с необязательной маской имени.</summary>
    public class FindCommand : ICommand
    {
        private const int FilterArguments = 3;
        private readonly VfsSession _session;

        /// <summary>Принимает общую сессию оболочки.</summary>
        public FindCommand(VfsSession session)
        {
            _session = session;
        }

        /// <summary>Поддерживает find [путь] и find путь -name маска.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            if (arguments.Length > 1 &&
                (arguments.Length != FilterArguments || arguments[1] != "-name"))
            {
                throw new ArgumentException("Использование: find [путь] [-name маска]");
            }

            string path = _session.Resolve(arguments.Length == 0 ? "." : arguments[0]);
            string pattern = arguments.Length == FilterArguments ? arguments[2] : "*";
            Regex mask = new Regex("\\A" + Regex.Escape(pattern).Replace("\\*", ".*")
                .Replace("\\?", ".") + "\\z", RegexOptions.CultureInvariant | RegexOptions.Singleline);
            Walk(path, mask, output);
            return CommandResult.Success;
        }

        /// <summary>Обходит дерево без рекурсии, включая начальную запись.</summary>
        private void Walk(string path, Regex mask, TextWriter output)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(path);
            VirtualFileSystem fileSystem = _session.GetFileSystem();
            while (pending.Count > 0)
            {
                string current = pending.Pop();
                string name = current == "/" ? "/" : current.Substring(current.LastIndexOf('/') + 1);
                if (mask.IsMatch(name))
                {
                    output.WriteLine(current);
                }

                if (fileSystem.GetEntry(current).IsDirectory())
                {
                    List<string> children = fileSystem.GetChildren(current);
                    for (int index = children.Count - 1; index >= 0; index--)
                    {
                        pending.Push(children[index]);
                    }
                }
            }
        }
    }
}
