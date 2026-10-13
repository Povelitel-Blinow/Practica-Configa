using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ShellEmulator.Vfs;

namespace ShellEmulator.Commands
{
    /// <summary>Переворачивает строки UTF-8-файлов или стандартного ввода.</summary>
    public class RevCommand : ICommand
    {
        private readonly VfsSession _session;
        private readonly TextReader _input;

        /// <summary>Принимает сессию и поток для режима без файлов.</summary>
        public RevCommand(VfsSession session, TextReader input)
        {
            _session = session;
            _input = input;
        }

        /// <summary>Читает файлы по порядку; без аргументов читает ввод до EOF.</summary>
        public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
        {
            if (arguments.Length == 0)
            {
                PrintLines(_input, output);
            }

            foreach (string argument in arguments)
            {
                VfsEntry entry = _session.GetFileSystem().GetEntry(_session.Resolve(argument));
                if (entry.IsDirectory())
                {
                    throw new IOException("Нельзя прочитать каталог: " + entry.GetPath());
                }

                string text = new UTF8Encoding(false, true).GetString(entry.ReadContent());
                using (StringReader reader = new StringReader(text))
                {
                    PrintLines(reader, output);
                }
            }

            return CommandResult.Success;
        }

        /// <summary>Переворачивает текстовые элементы, сохраняя составные символы Unicode.</summary>
        private static void PrintLines(TextReader reader, TextWriter output)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                List<string> elements = new List<string>();
                TextElementEnumerator iterator = StringInfo.GetTextElementEnumerator(line);
                while (iterator.MoveNext())
                {
                    elements.Add(iterator.GetTextElement());
                }

                for (int index = elements.Count - 1; index >= 0; index--)
                {
                    output.Write(elements[index]);
                }

                output.WriteLine();
            }
        }
    }
}
