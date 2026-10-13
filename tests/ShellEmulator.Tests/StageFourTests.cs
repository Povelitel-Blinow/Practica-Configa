using System;
using System.IO;
using System.Text;
using ShellEmulator.Vfs;

namespace ShellEmulator.Tests
{
    /// <summary>Проверяет файловые команды, переходы и ошибки этапа 4.</summary>
    internal class StageFourTests
    {
        private static int _sPassed;

        /// <summary>Запускает сценарии этапа 4.</summary>
        public static int Run()
        {
            _sPassed = 0;
            CheckNavigation();
            CheckFind();
            CheckRev();
            CheckErrors();
            return _sPassed;
        }

        /// <summary>Создаёт дерево с Unicode, пробелами, скрытым и двоичным файлами.</summary>
        private static VirtualFileSystem CreateFileSystem()
        {
            return new VirtualFileSystem(new VfsEntry[]
            {
                new VfsEntry("/dir", true, new byte[0]),
                new VfsEntry("/dir/empty", true, new byte[0]),
                new VfsEntry("/dir/a.txt", false, Encoding.UTF8.GetBytes("abc\r\nПривет\n\nA😀e\u0301")),
                new VfsEntry("/dir/.hidden", false, new byte[0]),
                new VfsEntry("/space name", false, Encoding.UTF8.GetBytes("xy")),
                new VfsEntry("/bad.bin", false, new byte[] {255})
            });
        }

        /// <summary>Проверяет относительные пути, корень и независимость от рабочего каталога ОС.</summary>
        private static void CheckNavigation()
        {
            StringWriter output = new StringWriter();
            Shell shell = new Shell(new StringReader(""), output, new StringWriter(), CreateFileSystem());
            string host = Environment.CurrentDirectory;
            Require(shell.ExecuteCommand("cd /dir") == CommandResult.Success, "cd absolute");
            Require(shell.ExecuteCommand("ls") == CommandResult.Success, "ls current");
            Require(output.ToString() == ".hidden" + Environment.NewLine + "a.txt" + Environment.NewLine
                + "empty" + Environment.NewLine, "sorted listing including hidden");
            output.GetStringBuilder().Clear();
            Require(shell.ExecuteCommand("cd missing") == CommandResult.Error, "failed cd");
            Require(shell.ExecuteCommand("ls ./a.txt") == CommandResult.Success, "failed cd preserves directory");
            Require(output.ToString() == "a.txt" + Environment.NewLine, "ls file");
            Require(shell.ExecuteCommand("cd empty/../..") == CommandResult.Success, "relative parents");
            Require(shell.ExecuteCommand("cd ../../..") == CommandResult.Success, "root parents");
            Require(shell.ExecuteCommand("ls 'space name'") == CommandResult.Success, "quoted path");
            Require(shell.ExecuteCommand("cd") == CommandResult.Success, "cd default root");
            Require(Environment.CurrentDirectory == host, "host directory unchanged");
        }

        /// <summary>Проверяет обход, маски, файлы и пустой результат.</summary>
        private static void CheckFind()
        {
            StringWriter output = new StringWriter();
            Shell shell = new Shell(new StringReader(""), output, new StringWriter(), CreateFileSystem());
            Require(shell.ExecuteCommand("find /dir -name '*.txt'") == CommandResult.Success, "find mask");
            Require(output.ToString() == "/dir/a.txt" + Environment.NewLine, "find txt result");
            output.GetStringBuilder().Clear();
            shell.ExecuteCommand("cd /dir");
            Require(shell.ExecuteCommand("find") == CommandResult.Success, "find default");
            Require(output.ToString() == "/dir\n/dir/.hidden\n/dir/a.txt\n/dir/empty\n"
                .Replace("\n", Environment.NewLine), "find sorted tree");
            output.GetStringBuilder().Clear();
            shell.ExecuteCommand("find a.txt -name '?.txt'");
            Require(output.ToString().Contains("/dir/a.txt"), "question mark mask");
            output.GetStringBuilder().Clear();
            shell.ExecuteCommand("find . -name absent");
            Require(output.ToString() == "", "no matches");
        }

        /// <summary>Проверяет строки, Unicode, несколько файлов и поток ввода.</summary>
        private static void CheckRev()
        {
            StringWriter output = new StringWriter();
            Shell shell = new Shell(new StringReader("abc\n\n"), output, new StringWriter(), CreateFileSystem());
            Require(shell.ExecuteCommand("rev /dir/a.txt '/space name'") == CommandResult.Success, "rev files");
            Require(output.ToString() == "cba\nтевирП\n\ne\u0301😀A\nyx\n"
                .Replace("\n", Environment.NewLine), "rev Unicode and lines");
            output.GetStringBuilder().Clear();
            Require(shell.ExecuteCommand("rev /dir/.hidden") == CommandResult.Success, "rev empty file");
            Require(output.ToString() == "", "empty file output");
            Require(shell.ExecuteCommand("rev") == CommandResult.Success, "rev input");
            Require(output.ToString() == "cba\n\n".Replace("\n", Environment.NewLine), "rev input output");
        }

        /// <summary>Ошибки не повреждают сессию и прекращают стартовый скрипт.</summary>
        private static void CheckErrors()
        {
            string[] commands = new string[] {"ls /missing", "ls ''", "cd /space\\name", "cd /space name",
                "cd /bad.bin", "ls /bad.bin/..", "ls /bad.bin/", "find / -bad x", "find / a",
                "rev /dir", "rev /bad.bin", "find /missing", "rev /missing"};
            foreach (string command in commands)
            {
                StringWriter output = new StringWriter();
                StringWriter error = new StringWriter();
                Shell shell = new Shell(new StringReader(""), output, error, CreateFileSystem());
                Require(shell.RunScript(new StringReader(command + "\nls NEVER\n")) == CommandResult.Error,
                    "error result: " + command);
                Require(error.ToString().Length > 0 && !output.ToString().Contains("NEVER"), "stop on error");
                Require(shell.ExecuteCommand("ls /") == CommandResult.Success, "recover after error");
            }
        }

        /// <summary>Считает успешные проверки и сообщает об ошибке.</summary>
        private static void Require(bool condition, string description)
        {
            if (!condition)
            {
                throw new InvalidOperationException(description);
            }

            _sPassed++;
        }
    }
}
