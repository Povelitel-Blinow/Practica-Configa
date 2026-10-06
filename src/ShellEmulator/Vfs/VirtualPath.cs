using System;
using System.IO;

namespace ShellEmulator.Vfs
{
    /// <summary>Правила абсолютных UNIX-путей внутри VFS, независимые от ОС хоста.</summary>
    public class VirtualPath
    {
        /// <summary>Отклоняет неоднозначные пути и управляющие символы.</summary>
        public static void Validate(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Путь VFS должен начинаться с /.");
            }

            if (path == "/")
            {
                return;
            }

            string[] parts = path.Substring(1).Split('/');
            foreach (string part in parts)
            {
                if (part.Length == 0 || part == "." || part == "..")
                {
                    throw new InvalidDataException("Некорректный путь VFS: " + path);
                }
            }

            foreach (char symbol in path)
            {
                if (char.IsControl(symbol) || symbol == '\\')
                {
                    throw new InvalidDataException("Недопустимый символ в пути VFS.");
                }
            }
        }

        /// <summary>Возвращает родителя корректного абсолютного пути; родитель корня — корень.</summary>
        public static string GetParent(string path)
        {
            Validate(path);
            int separator = path.LastIndexOf('/');
            if (separator == 0)
            {
                return "/";
            }

            return path.Substring(0, separator);
        }
    }
}
