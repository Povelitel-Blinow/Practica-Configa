using System;
using System.Text;

namespace ShellEmulator
{
    /// <summary>Точка входа в консольный эмулятор.</summary>
    internal class Program
    {
        /// <summary>Настраивает кодировку и запускает интерактивный цикл.</summary>
        private static int Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            return Application.Run(args, Console.In, Console.Out, Console.Error);
        }
    }
}
