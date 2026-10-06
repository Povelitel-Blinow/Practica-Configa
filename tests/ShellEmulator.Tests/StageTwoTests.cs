using System;
using System.IO;
using ShellEmulator.Commands;

namespace ShellEmulator.Tests
{
    /// <summary>Проверки конфигурации, сценариев и расширения реестра команд.</summary>
    internal class StageTwoTests
    {
        private static int s_passed;

        /// <summary>Запускает проверки второго этапа и возвращает их число.</summary>
        public static int Run()
        {
            s_passed = 0;
            CheckOptions();
            CheckInvalidOptions();
            CheckScriptErrors();
            CheckRegistry();
            CheckApplication();
            return s_passed;
        }

        /// <summary>Проверяет значения по умолчанию, оба параметра и произвольный порядок.</summary>
        private static void CheckOptions()
        {
            StartupOptions empty = StartupOptions.Parse(new string[] {});
            Require(empty.GetVfsPath() == "" && empty.GetScriptPath() == "", "default options");
            StartupOptions both = StartupOptions.Parse(
                new string[] {"--script", "folder/script.txt", "--vfs", "disk image.csv"});
            Require(both.GetVfsPath() == "disk image.csv", "VFS path with spaces");
            Require(both.GetScriptPath() == "folder/script.txt", "script path");
            StartupOptions single = StartupOptions.Parse(new string[] {"--vfs", "vfs.csv"});
            Require(single.GetScriptPath() == "", "optional script");
        }

        /// <summary>Проверяет отказ при неизвестных, пустых, повторных и неполных параметрах.</summary>
        private static void CheckInvalidOptions()
        {
            string[][] cases = new string[][]
            {
                new string[] {"--unknown", "x"},
                new string[] {"--vfs"},
                new string[] {"--script", ""},
                new string[] {"--vfs", "one", "--vfs", "two"},
                new string[] {"--script", "--vfs", "path"},
                new string[] {"extra"}
            };
            foreach (string[] arguments in cases)
            {
                StringWriter error = new StringWriter();
                int code = Application.Run(arguments, new StringReader(""), new StringWriter(), error);
                Require(code == 1 && error.ToString().Contains("Ошибка параметров"), "invalid options");
            }
        }

        /// <summary>Все виды ошибок прекращают скрипт до следующей команды.</summary>
        private static void CheckScriptErrors()
        {
            string[] errors = new string[] {"unknown", "cd a b", "exit 1", "ls \"unclosed"};
            foreach (string line in errors)
            {
                StringWriter output = new StringWriter();
                StringWriter error = new StringWriter();
                Shell shell = new Shell(new StringReader(""), output, error);
                CommandResult result = shell.RunScript(new StringReader("ls\n" + line + "\nls NEVER\n"));
                Require(result == CommandResult.Error, "script result");
                Require(output.ToString().Contains(Shell.GetPrompt() + line), "echo failing command");
                Require(output.ToString().Contains("ls: []"), "previous command runs");
                Require(!output.ToString().Contains("NEVER"), "stop before next line");
                Require(error.ToString().Length > 0, "script error message");
            }
        }

        /// <summary>Новая команда доступна без изменения диспетчера; повторы запрещены.</summary>
        private static void CheckRegistry()
        {
            StringWriter output = new StringWriter();
            Shell shell = new Shell(new StringReader(""), output, new StringWriter());
            shell.RegisterCommand("custom", new TestCommand());
            Require(shell.ExecuteCommand("custom value") == CommandResult.Success, "registered command");
            Require(output.ToString().Contains("value"), "registered arguments");
            try
            {
                shell.RegisterCommand("custom", new TestCommand());
            }
            catch (ArgumentException)
            {
                s_passed++;
                return;
            }

            throw new InvalidOperationException("Duplicate registration accepted");
        }

        /// <summary>Проверяет запуск с реальным UTF-8-файлом и освобождение ресурса.</summary>
        private static void CheckApplication()
        {
            string path = Path.Combine(Path.GetTempPath(), "shell test " + Guid.NewGuid() + ".txt");
            try
            {
                File.WriteAllText(path, "ls \"папка\"\ncd\n");
                CheckSuccessfulApplication(path);
                File.WriteAllText(path, "exit\nls NEVER\n");
                CheckStoppedApplication(path, 0);
                File.WriteAllText(path, "unknown\nls NEVER\n");
                CheckStoppedApplication(path, 1);
            }
            finally
            {
                File.Delete(path);
            }

            StringWriter error = new StringWriter();
            int code = Application.Run(new string[] {"--script", path},
                new StringReader(""), new StringWriter(), error);
            Require(code == 1 && error.ToString().Contains("Ошибка чтения"), "missing script");
        }

        /// <summary>После успешного скрипта запускается REPL; все параметры напечатаны.</summary>
        private static void CheckSuccessfulApplication(string path)
        {
            StringWriter output = new StringWriter();
            StringWriter error = new StringWriter();
            int code = Application.Run(new string[] {"--vfs", "not loaded.csv", "--script", path},
                new StringReader("ls INTERACTIVE\nexit\n"), output, error);
            Require(code == 0 && error.ToString() == "", "successful startup");
            Require(output.ToString().Contains("VFS: not loaded.csv"), "debug VFS");
            Require(output.ToString().Contains("Script: " + path), "debug script");
            Require(output.ToString().Contains("ls \"папка\""), "UTF-8 script input");
            Require(output.ToString().Contains("INTERACTIVE"), "REPL after script");
        }

        /// <summary>После exit или ошибки скрипта REPL не запускается.</summary>
        private static void CheckStoppedApplication(string path, int expectedCode)
        {
            StringWriter output = new StringWriter();
            int code = Application.Run(new string[] {"--script", path},
                new StringReader("ls INTERACTIVE\n"), output, new StringWriter());
            Require(code == expectedCode, "startup exit code");
            Require(!output.ToString().Contains("NEVER"), "script stopped");
            Require(!output.ToString().Contains("INTERACTIVE"), "REPL not started");
        }

        /// <summary>Регистрирует успешную проверку или сообщает о провале.</summary>
        private static void Require(bool condition, string description)
        {
            if (!condition)
            {
                throw new InvalidOperationException(description);
            }

            s_passed++;
        }

        /// <summary>Тестовый обработчик для проверки расширения реестра.</summary>
        private class TestCommand : ICommand
        {
            /// <summary>Выводит первый аргумент.</summary>
            public CommandResult Execute(string[] arguments, TextWriter output, TextWriter error)
            {
                output.WriteLine(arguments[0]);
                return CommandResult.Success;
            }
        }
    }
}
