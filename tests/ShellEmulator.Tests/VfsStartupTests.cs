using System;
using System.IO;
using System.Text;
using ShellEmulator.Vfs;

namespace ShellEmulator.Tests
{
    /// <summary>Проверки приветствия, порядка запуска и ошибок образа VFS.</summary>
    internal class VfsStartupTests
    {
        private const string Header = "path,type,content\n";
        private static int s_passed;

        /// <summary>Запускает проверки интеграции и возвращает их число.</summary>
        public static int Run()
        {
            s_passed = 0;
            CheckMessages();
            string path = Path.Combine(Path.GetTempPath(), "vfs startup " + Guid.NewGuid() + ".csv");
            try
            {
                CheckStartupOrder(path);
                CheckInvalidStartup(path);
            }
            finally
            {
                File.Delete(path);
                File.Delete(path + ".txt");
            }

            CheckMissingFile(path);
            return s_passed;
        }

        /// <summary>Проверяет отсутствие motd, каталог, вложенный файл и варианты окончания текста.</summary>
        private static void CheckMessages()
        {
            Require(Message(Header) == "", "no motd");
            Require(Message(Header + "/motd,directory,\n") == "", "motd directory ignored");
            Require(Message(Header + "/a,directory,\n/a/motd,file,YQ==\n") == "", "nested motd ignored");
            Require(Message(Header + "/motd,file,\n") == "", "empty motd");
            Require(Message(Header + "/motd,file,YQo=\n") == "a\n", "motd preserves final newline");
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("Привет!"));
            Require(Message(Header + "/motd,file," + encoded + "\n") == "Привет!" + Environment.NewLine,
                "UTF-8 motd and missing newline");
        }

        /// <summary>Приветствие предшествует скрипту, а данные CSV остаются неизменными.</summary>
        private static void CheckStartupOrder(string path)
        {
            string csv = Header + "/motd,file,V0VMQ09NRQo=\n";
            File.WriteAllText(path, csv);
            File.WriteAllText(path + ".txt", "ls\nexit\n");
            StringWriter output = new StringWriter();
            StringWriter error = new StringWriter();
            int code = Application.Run(new string[] {"--vfs", path, "--script", path + ".txt"},
                new StringReader(""), output, error);
            string text = output.ToString();
            Require(code == 0 && error.ToString() == "", "valid startup");
            Require(text.IndexOf("WELCOME", StringComparison.Ordinal)
                < text.IndexOf(Shell.GetPrompt() + "ls", StringComparison.Ordinal), "motd before script");
            Require(text.Contains("ls: []"), "commands still available");
            Require(File.ReadAllText(path) == csv, "application preserves source");
        }

        /// <summary>Ошибки CSV и текста motd не допускают исполнения скрипта или REPL.</summary>
        private static void CheckInvalidStartup(string path)
        {
            File.WriteAllText(path + ".txt", "ls NEVER\n");
            string[] images = new string[] {"bad header", Header + "/motd,file,/w==\n"};
            foreach (string csv in images)
            {
                File.WriteAllText(path, csv);
                CheckRejectedStartup(path);
            }

            File.WriteAllBytes(path, new byte[] {255, 255, 255});
            CheckRejectedStartup(path);
        }

        /// <summary>Проверяет код ошибки и отсутствие запуска команд при повреждённом образе.</summary>
        private static void CheckRejectedStartup(string path)
        {
            StringWriter output = new StringWriter();
            StringWriter error = new StringWriter();
            int code = Application.Run(new string[] {"--vfs", path, "--script", path + ".txt"},
                new StringReader("ls INTERACTIVE\n"), output, error);
            Require(code == 1 && error.ToString().Contains("Ошибка VFS"), "invalid image diagnostics");
            Require(!output.ToString().Contains("NEVER"), "script not started");
            Require(!output.ToString().Contains("INTERACTIVE"), "REPL not started");
        }

        /// <summary>Отсутствующий источник VFS даёт ошибку чтения с ненулевым кодом.</summary>
        private static void CheckMissingFile(string path)
        {
            StringWriter error = new StringWriter();
            int code = Application.Run(new string[] {"--vfs", path},
                new StringReader(""), new StringWriter(), error);
            Require(code == 1 && error.ToString().Contains("Ошибка чтения"), "missing VFS");
        }

        /// <summary>Возвращает вывод motd для CSV в памяти.</summary>
        private static string Message(string csv)
        {
            VirtualFileSystem vfs = new CsvVfsLoader().Read(new StringReader(csv));
            StringWriter output = new StringWriter();
            VfsStartup.PrintMessage(vfs, output);
            return output.ToString();
        }

        /// <summary>Учитывает проверку либо сообщает о её провале.</summary>
        private static void Require(bool condition, string description)
        {
            if (!condition)
            {
                throw new InvalidOperationException(description);
            }

            s_passed++;
        }
    }
}
