using System.IO;
using System.Text;

namespace ShellEmulator.Vfs
{
    /// <summary>Подготавливает VFS и приветствие до выполнения стартового скрипта.</summary>
    public class VfsStartup
    {
        /// <summary>Загружает указанный CSV либо создаёт пустую VFS.</summary>
        public static VirtualFileSystem Load(string path)
        {
            if (path.Length == 0)
            {
                return new VirtualFileSystem();
            }

            CsvVfsLoader loader = new CsvVfsLoader();
            return loader.Load(path);
        }

        /// <summary>Выводит только файл /motd, декодируя его содержимое как UTF-8.</summary>
        public static void PrintMessage(VirtualFileSystem fileSystem, TextWriter output)
        {
            if (!fileSystem.Contains("/motd") || fileSystem.GetEntry("/motd").IsDirectory())
            {
                return;
            }

            try
            {
                UTF8Encoding encoding = new UTF8Encoding(false, true);
                string message = encoding.GetString(fileSystem.GetEntry("/motd").ReadContent());
                output.Write(message);
                if (message.Length != 0 && !message.EndsWith("\n"))
                {
                    output.WriteLine();
                }
            }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException("Файл /motd должен содержать текст UTF-8.", exception);
            }
        }
    }
}
