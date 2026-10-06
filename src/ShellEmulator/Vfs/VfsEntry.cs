using System;
using System.IO;

namespace ShellEmulator.Vfs
{
    /// <summary>Запись VFS с неизменяемыми метаданными и содержимым в памяти.</summary>
    public class VfsEntry
    {
        private readonly string _path;
        private readonly bool _isDirectory;
        private readonly byte[] _content;

        /// <summary>Проверяет запись и копирует данные, исключая внешнее изменение массива.</summary>
        public VfsEntry(string path, bool isDirectory, byte[] content)
        {
            VirtualPath.Validate(path);
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (isDirectory && content.Length != 0)
            {
                throw new InvalidDataException("Каталог не может содержать данные файла: " + path);
            }

            _path = path;
            _isDirectory = isDirectory;
            _content = (byte[])content.Clone();
        }

        /// <summary>Возвращает абсолютный путь записи.</summary>
        public string GetPath()
        {
            return _path;
        }

        /// <summary>Отличает каталог от файла.</summary>
        public bool IsDirectory()
        {
            return _isDirectory;
        }

        /// <summary>Возвращает независимую копию содержимого файла.</summary>
        public byte[] ReadContent()
        {
            if (_isDirectory)
            {
                throw new InvalidOperationException("Нельзя прочитать каталог как файл: " + _path);
            }

            return (byte[])_content.Clone();
        }
    }
}
