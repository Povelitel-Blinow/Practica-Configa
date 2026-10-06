using System;
using System.IO;
using ShellEmulator.Vfs;

namespace ShellEmulator
{
    /// <summary>Запускает конфигурацию, стартовый скрипт и интерактивный режим.</summary>
    public class Application
    {
        /// <summary>Возвращает 0 при штатном завершении и 1 при ошибке запуска или скрипта.</summary>
        public static int Run(string[] args, TextReader input, TextWriter output, TextWriter error)
        {
            try
            {
                StartupOptions options = StartupOptions.Parse(args);
                return Start(options, input, output, error);
            }
            catch (InvalidDataException exception)
            {
                error.WriteLine("Ошибка VFS: " + exception.Message);
            }
            catch (ArgumentException exception)
            {
                error.WriteLine("Ошибка параметров: " + exception.Message);
            }
            catch (IOException exception)
            {
                error.WriteLine("Ошибка чтения: " + exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                error.WriteLine("Ошибка доступа: " + exception.Message);
            }

            return 1;
        }

        /// <summary>Подключает VFS, печатает motd и запускает скрипт перед интерактивным циклом.</summary>
        private static int Start(StartupOptions options, TextReader input, TextWriter output, TextWriter error)
        {
            output.WriteLine("VFS: " + options.GetVfsPath());
            output.WriteLine("Script: " + options.GetScriptPath());
            VirtualFileSystem fileSystem = VfsStartup.Load(options.GetVfsPath());
            VfsStartup.PrintMessage(fileSystem, output);
            Shell shell = new Shell(input, output, error, fileSystem);
            CommandResult result = RunStartupScript(options.GetScriptPath(), shell);
            if (result == CommandResult.Error)
            {
                return 1;
            }

            if (result != CommandResult.Exit)
            {
                shell.Run();
            }

            return 0;
        }

        /// <summary>Открывает скрипт в UTF-8 и гарантированно закрывает файл.</summary>
        private static CommandResult RunStartupScript(string path, Shell shell)
        {
            if (path.Length == 0)
            {
                return CommandResult.Success;
            }

            using (StreamReader reader = new StreamReader(path))
            {
                return shell.RunScript(reader);
            }
        }
    }
}
