using System;
using System.Collections.Generic;
using System.IO;

namespace ShellEmulator.Vfs
{
    /// <summary>Индекс файлов и каталогов в памяти; не обращается к файловой системе хоста.</summary>
    public class VirtualFileSystem
    {
        private readonly Dictionary<string, VfsEntry> _entries;

        /// <summary>Создаёт пустую VFS с корневым каталогом.</summary>
        public VirtualFileSystem() : this(new List<VfsEntry>())
        {
        }

        /// <summary>Индексирует записи и проверяет дерево независимо от порядка строк CSV.</summary>
        public VirtualFileSystem(IEnumerable<VfsEntry> entries)
        {
            _entries = new Dictionary<string, VfsEntry>(StringComparer.Ordinal);
            foreach (VfsEntry entry in entries)
            {
                if (_entries.ContainsKey(entry.GetPath()))
                {
                    throw new InvalidDataException("Повторный путь VFS: " + entry.GetPath());
                }

                _entries.Add(entry.GetPath(), entry);
            }

            if (!_entries.ContainsKey("/"))
            {
                _entries.Add("/", new VfsEntry("/", true, new byte[0]));
            }

            ValidateTree();
        }

        /// <summary>Проверяет наличие записи с точным регистрозависимым путём.</summary>
        public bool Contains(string path)
        {
            VirtualPath.Validate(path);
            return _entries.ContainsKey(path);
        }

        /// <summary>Возвращает запись только из памяти.</summary>
        public VfsEntry GetEntry(string path)
        {
            VirtualPath.Validate(path);
            VfsEntry entry;
            if (!_entries.TryGetValue(path, out entry))
            {
                throw new FileNotFoundException("Путь отсутствует в VFS: " + path);
            }

            return entry;
        }

        /// <summary>Проверяет корень и существование каждого родительского каталога.</summary>
        private void ValidateTree()
        {
            if (!_entries["/"].IsDirectory())
            {
                throw new InvalidDataException("Корень VFS должен быть каталогом.");
            }

            foreach (VfsEntry entry in _entries.Values)
            {
                string parentPath = VirtualPath.GetParent(entry.GetPath());
                VfsEntry parent;
                if (!_entries.TryGetValue(parentPath, out parent) || !parent.IsDirectory())
                {
                    throw new InvalidDataException("Отсутствует родительский каталог: " + parentPath);
                }
            }
        }
    }
}
