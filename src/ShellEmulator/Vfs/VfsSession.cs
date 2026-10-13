using System;
using System.IO;

namespace ShellEmulator.Vfs
{
    /// <summary>Рабочий каталог и разрешение путей внутри VFS.</summary>
    public class VfsSession
    {
        private readonly VirtualFileSystem _fileSystem;
        private string _directory = "/";

        /// <summary>Создаёт сессию в корне VFS.</summary>
        public VfsSession(VirtualFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>Возвращает рабочий каталог.</summary>
        public string GetDirectory()
        {
            return _directory;
        }

        /// <summary>Возвращает модель VFS.</summary>
        public VirtualFileSystem GetFileSystem()
        {
            return _fileSystem;
        }

        /// <summary>Разрешает путь с точками, проверяя каждый промежуточный каталог.</summary>
        public string Resolve(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Путь не может быть пустым.");
            }

            string result = path.StartsWith("/", StringComparison.Ordinal) ? "/" : _directory;
            foreach (string part in path.Split('/'))
            {
                if (part.Length == 0)
                {
                    continue;
                }

                RequireDirectory(result);
                if (part == ".")
                {
                    continue;
                }

                result = part == ".." ? VirtualPath.GetParent(result)
                    : (result == "/" ? "/" : result + "/") + part;
                _fileSystem.GetEntry(result);
            }

            if (path.EndsWith("/", StringComparison.Ordinal))
            {
                RequireDirectory(result);
            }

            return result;
        }

        /// <summary>Меняет каталог только после успешной проверки.</summary>
        public void ChangeDirectory(string path)
        {
            string resolved = Resolve(path);
            RequireDirectory(resolved);
            _directory = resolved;
        }

        /// <summary>Проверяет, что запись является каталогом.</summary>
        private void RequireDirectory(string path)
        {
            if (!_fileSystem.GetEntry(path).IsDirectory())
            {
                throw new IOException("Не является каталогом: " + path);
            }
        }
    }
}
