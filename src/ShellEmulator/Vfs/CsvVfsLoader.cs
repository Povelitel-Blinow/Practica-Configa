using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace ShellEmulator.Vfs
{
    /// <summary>Читает CSV path,type,content; все данные файлов представлены в Base64.</summary>
    public class CsvVfsLoader
    {
        private const int FieldCount = 3;
        private const int PathField = 0;
        private const int TypeField = 1;
        private const int ContentField = 2;

        /// <summary>Читает файл один раз и закрывает его, возвращая независимую VFS в памяти.</summary>
        public VirtualFileSystem Load(string path)
        {
            using (StreamReader reader = new StreamReader(path, new UTF8Encoding(false, true)))
            {
                return Read(reader);
            }
        }

        /// <summary>Разбирает CSV стандартным парсером .NET с поддержкой кавычек и запятых.</summary>
        public VirtualFileSystem Read(TextReader reader)
        {
            try
            {
                using (TextFieldParser parser = new TextFieldParser(reader))
                {
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.TrimWhiteSpace = false;
                    ValidateHeader(parser.ReadFields());
                    return new VirtualFileSystem(ReadEntries(parser));
                }
            }
            catch (MalformedLineException exception)
            {
                throw new InvalidDataException("Некорректная строка CSV: " + exception.Message, exception);
            }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException("CSV должен использовать кодировку UTF-8.", exception);
            }
        }

        /// <summary>Проверяет обязательный заголовок, порядок и количество столбцов.</summary>
        private static void ValidateHeader(string[] fields)
        {
            if (fields == null || fields.Length != FieldCount)
            {
                throw new InvalidDataException("Ожидается заголовок CSV: path,type,content");
            }

            if (fields[PathField] != "path" || fields[TypeField] != "type" || fields[ContentField] != "content")
            {
                throw new InvalidDataException("Ожидается заголовок CSV: path,type,content");
            }
        }

        /// <summary>Собирает записи, указывая номер физической строки при ошибке содержимого.</summary>
        private static List<VfsEntry> ReadEntries(TextFieldParser parser)
        {
            List<VfsEntry> entries = new List<VfsEntry>();
            while (!parser.EndOfData)
            {
                long line = parser.LineNumber;
                try
                {
                    entries.Add(ParseEntry(parser.ReadFields()));
                }
                catch (InvalidDataException exception)
                {
                    throw new InvalidDataException("Строка CSV " + line + ": " + exception.Message, exception);
                }
            }

            return entries;
        }

        /// <summary>Проверяет тип записи и декодирует Base64 без записи данных на диск.</summary>
        private static VfsEntry ParseEntry(string[] fields)
        {
            if (fields == null || fields.Length != FieldCount)
            {
                throw new InvalidDataException("Запись должна содержать ровно три столбца.");
            }

            string type = fields[TypeField];
            if (type != "directory" && type != "file")
            {
                throw new InvalidDataException("Неизвестный тип записи: " + type);
            }

            if (type == "directory" && fields[ContentField].Length != 0)
            {
                throw new InvalidDataException("Поле content каталога должно быть пустым.");
            }

            try
            {
                byte[] content = Convert.FromBase64String(fields[ContentField]);
                return new VfsEntry(fields[PathField], type == "directory", content);
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException("Некорректный Base64: " + fields[PathField], exception);
            }
        }
    }
}
