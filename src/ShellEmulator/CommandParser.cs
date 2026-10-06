using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShellEmulator
{

    /// <summary>Разбирает аргументы с кавычками и раскрывает переменные окружения ОС.</summary>
    public class CommandParser
    {
        private static readonly Regex s_fragment = new Regex("[^\\s'\"]+|'[^']*'|\"[^\"]*\"");
        private static readonly Regex s_variable = new Regex(
            "\\$\\{([A-Za-z_][A-Za-z0-9_]*)\\}|\\$([A-Za-z_][A-Za-z0-9_]*)");

        private const int QuoteCount = 2;
        private const int BracedNameGroup = 1;
        private const int PlainNameGroup = 2;

        /// <summary>Возвращает команду и аргументы; неизвестные переменные заменяет пустой строкой.</summary>
        /// <exception cref="FormatException">Кавычки не закрыты.</exception>
        public static string[] Parse(string line)
        {
            List<string> arguments = new List<string>();
            StringBuilder word = new StringBuilder();
            bool wordStarted = false;
            int position = 0;
            while (position < line.Length)
            {
                if (char.IsWhiteSpace(line[position]))
                {
                    AddWord(arguments, word, wordStarted);
                    wordStarted = false;
                    position++;
                    continue;
                }

                Match match = s_fragment.Match(line, position);
                if (!match.Success || match.Index != position)
                {
                    throw new FormatException("Незакрытая кавычка.");
                }

                word.Append(ExpandFragment(match.Value));
                wordStarted = true;
                position += match.Length;
            }

            AddWord(arguments, word, wordStarted);
            return arguments.ToArray();
        }

        /// <summary>Обрабатывает кавычки и подставляет значения без повторного разбора.</summary>
        private static string ExpandFragment(string fragment)
        {
            if (fragment[0] == '\'')
            {
                return fragment.Substring(1, fragment.Length - QuoteCount);
            }

            string value = fragment;
            if (fragment[0] == '"')
            {
                value = fragment.Substring(1, fragment.Length - QuoteCount);
            }

            return ExpandVariables(value);
        }

        /// <summary>Собирает строку с подстановками без повторного раскрытия их значений.</summary>
        private static string ExpandVariables(string value)
        {
            StringBuilder result = new StringBuilder();
            MatchCollection matches = s_variable.Matches(value);
            int position = 0;
            for (int index = 0; index < matches.Count; index++)
            {
                Match match = matches[index];
                result.Append(value.Substring(position, match.Index - position));
                result.Append(ExpandVariable(match));
                position = match.Index + match.Length;
            }

            result.Append(value.Substring(position));
            return result.ToString();
        }

        /// <summary>Получает значение одной переменной из окружения ОС.</summary>
        private static string ExpandVariable(Match match)
        {
            string name = match.Groups[PlainNameGroup].Value;
            if (match.Groups[BracedNameGroup].Success)
            {
                name = match.Groups[BracedNameGroup].Value;
            }

            string value = Environment.GetEnvironmentVariable(name);
            if (value == null)
            {
                return string.Empty;
            }

            return value;
        }

        /// <summary>Завершает аргумент, сохраняя явно заданные пустые строки.</summary>
        private static void AddWord(List<string> arguments, StringBuilder word, bool wordStarted)
        {
            if (!wordStarted)
            {
                return;
            }

            arguments.Add(word.ToString());
            word.Clear();
        }
    }
}
