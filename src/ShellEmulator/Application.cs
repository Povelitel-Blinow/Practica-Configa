using System;
using System.IO;

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
                output.WriteLine("VFS: " + options.GetVfsPath());
                output.WriteLine("Script: " + options.GetScriptPath());
                Shell shell = new Shell(input, output, error);
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
