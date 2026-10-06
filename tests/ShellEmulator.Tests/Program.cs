using System;
using System.IO;
using ShellEmulator;

namespace ShellEmulator.Tests
{

    /// <summary>Автономные проверки без сторонних пакетов; ненулевой код означает ошибку.</summary>
    internal class Program
    {
        private const string VariableName = "SHELL_EMULATOR_TEST_VALUE";
        private static int s_passed;

        /// <summary>Запускает проверки парсера, команд и полного цикла REPL.</summary>
        private static int Main()
        {
            string originalValue = Environment.GetEnvironmentVariable(VariableName);
            try
            {
                Environment.SetEnvironmentVariable(VariableName, "folder with spaces");
                CheckParser();
                CheckCommands();
                CheckRepl();
                s_passed += StageTwoTests.Run();
                s_passed += VfsTests.Run();
                s_passed += VfsStartupTests.Run();
                Console.WriteLine("PASS: " + s_passed + " checks");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("FAIL: " + exception.Message);
                return 1;
            }
            finally
            {
                Environment.SetEnvironmentVariable(VariableName, originalValue);
            }
        }

        /// <summary>Проверяет пробелы, кавычки, подстановки и синтаксические ошибки.</summary>
        private static void CheckParser()
        {
            Equal(new string[] {}, CommandParser.Parse(" \t "));
            Equal(new string[] {"ls", "-l", "/tmp"}, CommandParser.Parse(" ls\t-l  /tmp "));
            Equal(new string[] {"cd", "my folder"}, CommandParser.Parse("cd \"my folder\""));
            Equal(new string[] {"ls", "", "ab cd"}, CommandParser.Parse("ls '' a\"b c\"d"));
            Equal(new string[] {"cd", "folder with spaces"}, CommandParser.Parse("cd $" + VariableName));
            Equal(new string[] {"ls", "folder with spaces/file"},
                CommandParser.Parse("ls ${" + VariableName + "}/file"));
            Equal(new string[] {"ls", "folder with spaces"}, CommandParser.Parse("ls \"$" + VariableName + "\""));
            Equal(new string[] {"ls", "$" + VariableName}, CommandParser.Parse("ls '$" + VariableName + "'"));
            Environment.SetEnvironmentVariable(VariableName, null);
            Equal(new string[] {"ls", ""}, CommandParser.Parse("ls $" + VariableName));
            Environment.SetEnvironmentVariable(VariableName, "$HOME");
            Equal(new string[] {"ls", "$HOME"}, CommandParser.Parse("ls $" + VariableName));
            ExpectParseError("ls 'unclosed");
            ExpectParseError("ls \"unclosed");
        }

        /// <summary>Проверяет вывод заглушек, ошибки аргументов и завершение.</summary>
        private static void CheckCommands()
        {
            StringWriter output = new StringWriter();
            StringWriter errors = new StringWriter();
            Shell shell = new Shell(new StringReader(""), output, errors);
            string directory = Environment.CurrentDirectory;
            Require(shell.Execute("ls -l /tmp"), "ls continues");
            Require(output.ToString().Contains("ls: [\"-l\",\"/tmp\"]"), "ls output");
            Require(shell.Execute("cd /nonexistent"), "cd continues");
            Require(output.ToString().Contains("cd: [\"/nonexistent\"]"), "cd output");
            Require(Environment.CurrentDirectory == directory, "cd is a stub");
            Require(shell.Execute("cd"), "cd without arguments");
            Require(shell.Execute("cd one two"), "invalid cd continues");
            Require(shell.Execute("exit 1"), "invalid exit continues");
            Require(shell.Execute("unknown"), "unknown command continues");
            Require(shell.Execute("ls '"), "parse error continues");
            Require(errors.ToString().Contains("cd принимает"), "cd error");
            Require(errors.ToString().Contains("exit не принимает"), "exit error");
            Require(errors.ToString().Contains("неизвестная команда: unknown"), "unknown error");
            Require(errors.ToString().Contains("Незакрытая кавычка"), "quote error");
            Require(!shell.Execute("exit"), "exit stops");
        }

        /// <summary>Проверяет реальное приглашение, восстановление после ошибки, exit и EOF.</summary>
        private static void CheckRepl()
        {
            StringWriter output = new StringWriter();
            StringWriter errors = new StringWriter();
            new Shell(new StringReader("unknown\nls\nexit\ncd ignored\n"), output, errors).Run();
            string expectedPrompt = Environment.UserName + "@" + Environment.MachineName + ":~$ ";
            Require(output.ToString().StartsWith(expectedPrompt), "prompt");
            Require(output.ToString().Contains("ls: []"), "REPL continues after error");
            Require(!output.ToString().Contains("ignored"), "REPL stops on exit");
            StringWriter eofOutput = new StringWriter();
            new Shell(new StringReader(""), eofOutput, errors).Run();
            Require(eofOutput.ToString() == Shell.GetPrompt(), "EOF stops");
        }

        /// <summary>Сравнивает аргументы с ожидаемым результатом.</summary>
        private static void Equal(string[] expected, string[] actual)
        {
            if (expected.Length != actual.Length)
            {
                throw new InvalidOperationException("Different argument counts");
            }

            for (int index = 0; index < expected.Length; index++)
            {
                if (expected[index] != actual[index])
                {
                    throw new InvalidOperationException("Different argument at index " + index);
                }
            }

            s_passed++;
        }

        /// <summary>Проверяет отказ парсера при незакрытых кавычках.</summary>
        private static void ExpectParseError(string line)
        {
            try
            {
                CommandParser.Parse(line);
            }
            catch (FormatException)
            {
                s_passed++;
                return;
            }

            throw new InvalidOperationException("Expected FormatException");
        }

        /// <summary>Регистрирует успешную проверку либо прерывает тесты.</summary>
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
