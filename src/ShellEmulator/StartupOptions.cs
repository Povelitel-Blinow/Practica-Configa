using System;
using System.Collections.Generic;

namespace ShellEmulator
{
    /// <summary>Параметры запуска; VFS на этапе 2 только сохраняется в конфигурации.</summary>
    public class StartupOptions
    {
        private readonly Dictionary<string, string> _values;

        /// <summary>Создаёт конфигурацию с необязательными путями.</summary>
        public StartupOptions()
        {
            _values = new Dictionary<string, string>(StringComparer.Ordinal);
            _values.Add("--vfs", string.Empty);
            _values.Add("--script", string.Empty);
        }

        /// <summary>Возвращает путь VFS или пустую строку.</summary>
        public string GetVfsPath()
        {
            return _values["--vfs"];
        }

        /// <summary>Возвращает путь стартового скрипта или пустую строку.</summary>
        public string GetScriptPath()
        {
            return _values["--script"];
        }

        /// <summary>Разбирает пары имя-значение, отклоняя неизвестные и повторные параметры.</summary>
        public static StartupOptions Parse(string[] arguments)
        {
            StartupOptions options = new StartupOptions();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            int position = 0;
            while (position < arguments.Length)
            {
                string name = arguments[position];
                if (!options._values.ContainsKey(name))
                {
                    throw new ArgumentException("Неизвестный параметр: " + name);
                }

                if (!seen.Add(name))
                {
                    throw new ArgumentException("Повторный параметр: " + name);
                }

                position++;
                if (position == arguments.Length || string.IsNullOrWhiteSpace(arguments[position]))
                {
                    throw new ArgumentException("Не задан путь для " + name);
                }

                if (arguments[position].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException("Ожидался путь для " + name);
                }

                options._values[name] = arguments[position];
                position++;
            }

            return options;
        }
    }
}
