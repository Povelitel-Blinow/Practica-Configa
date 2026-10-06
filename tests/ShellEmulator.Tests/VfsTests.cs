using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ShellEmulator.Vfs;

namespace ShellEmulator.Tests
{
    /// <summary>Проверяет CSV, дерево VFS и независимость содержимого от внешних данных.</summary>
    internal class VfsTests
    {
        private const string Header = "path,type,content\n";
        private static int s_passed;

        /// <summary>Запускает проверки файловой модели и загрузчика.</summary>
        public static int Run()
        {
            s_passed = 0;
            CheckValidImages();
            CheckInvalidImages();
            CheckInvalidPaths();
            CheckContentIsolation();
            CheckSourceIndependence();
            return s_passed;
        }

        /// <summary>Проверяет минимальный образ, вложенность, CSV-кавычки и бинарные файлы.</summary>
        private static void CheckValidImages()
        {
            VirtualFileSystem empty = Read(Header);
            Require(empty.GetEntry("/").IsDirectory(), "implicit root");
            VirtualFileSystem explicitRoot = Read(Header + "/,directory,\n");
            Require(explicitRoot.Contains("/"), "explicit root");
            string csv = Header + "/a/b/c/data.bin,file,AAEC/w==\n/a/b/c,directory,\n"
                + "/a/b,directory,\n/a,directory,\n\"/comma,name\",file,\n"
                + "\"/say\"\"hello\",file,\n/Папка с пробелом,directory,\n/A,file,\n";
            VirtualFileSystem vfs = Read(csv);
            byte[] data = vfs.GetEntry("/a/b/c/data.bin").ReadContent();
            Require(Convert.ToBase64String(data) == "AAEC/w==", "binary round trip");
            Require(vfs.GetEntry("/comma,name").ReadContent().Length == 0, "quoted comma and empty file");
            Require(vfs.Contains("/say\"hello"), "escaped CSV quote");
            Require(vfs.Contains("/Папка с пробелом"), "unicode path");
            Require(vfs.GetEntry("/a").IsDirectory() && !vfs.GetEntry("/A").IsDirectory(), "case sensitivity");
            Shell shell = new Shell(new StringReader(""), new StringWriter(), new StringWriter(), vfs);
            Require(object.ReferenceEquals(shell.GetFileSystem(), vfs), "session retains loaded VFS");
        }

        /// <summary>Отклоняет некорректную структуру CSV, содержимое и связи между записями.</summary>
        private static void CheckInvalidImages()
        {
            string[] images = new string[]
            {
                "", "wrong,type,content\n", Header + "/a,file\n", Header + "/a,file,,extra\n",
                Header + "/a,link,\n", Header + "/a,file,NOT_BASE64\n",
                Header + "/a,directory,YQ==\n", Header + "/a,file,\n/a,file,\n",
                Header + "/a/b,file,\n", Header + "/a,file,\n/a/b,file,\n",
                Header + "/,file,\n", Header + "/,directory,\n/,directory,\n",
                Header + "\"/unclosed,file,\n", Header + "\"/a\"oops,file,\n"
            };
            foreach (string csv in images)
            {
                ExpectInvalid(csv);
            }
        }

        /// <summary>Исключает пути хоста, обход родителей и неоднозначные разделители.</summary>
        private static void CheckInvalidPaths()
        {
            string[] paths = new string[] {"relative", "/a/../b", "/./b", "/a//b", "/a/", "/a\\b", ""};
            foreach (string path in paths)
            {
                ExpectInvalid(Header + path + ",file,\n");
            }

            ExpectInvalid(Header + "\"/a\nb\",file,\n");
        }

        /// <summary>Изменение входного массива и прочитанной копии не меняет состояние VFS.</summary>
        private static void CheckContentIsolation()
        {
            byte[] source = Encoding.UTF8.GetBytes("original");
            VfsEntry entry = new VfsEntry("/data", false, source);
            source[0] = 0;
            byte[] copy = entry.ReadContent();
            copy[0] = 0;
            Require(Encoding.UTF8.GetString(entry.ReadContent()) == "original", "defensive byte copies");
            List<VfsEntry> entries = new List<VfsEntry>();
            entries.Add(entry);
            VirtualFileSystem vfs = new VirtualFileSystem(entries);
            entries.Clear();
            Require(vfs.Contains("/data"), "independent entry index");
            try
            {
                vfs.GetEntry("/").ReadContent();
            }
            catch (InvalidOperationException)
            {
                s_passed++;
                return;
            }

            throw new InvalidOperationException("Directory accepted as file");
        }

        /// <summary>Загруженные данные доступны после удаления источника; загрузка его не меняет.</summary>
        private static void CheckSourceIndependence()
        {
            string path = Path.Combine(Path.GetTempPath(), "vfs-test-" + Guid.NewGuid() + ".csv");
            string csv = Header + "/data,file,YWJj\n";
            try
            {
                File.WriteAllText(path, csv, new UTF8Encoding(true));
                string before = Convert.ToBase64String(File.ReadAllBytes(path));
                VirtualFileSystem vfs = new CsvVfsLoader().Load(path);
                Require(Convert.ToBase64String(File.ReadAllBytes(path)) == before, "source unchanged and BOM read");
                File.Delete(path);
                Require(Encoding.UTF8.GetString(vfs.GetEntry("/data").ReadContent()) == "abc", "memory only");
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>Читает тестовый образ без обращения к диску.</summary>
        private static VirtualFileSystem Read(string csv)
        {
            return new CsvVfsLoader().Read(new StringReader(csv));
        }

        /// <summary>Проверяет диагностируемый отказ загрузчика.</summary>
        private static void ExpectInvalid(string csv)
        {
            try
            {
                Read(csv);
            }
            catch (InvalidDataException)
            {
                s_passed++;
                return;
            }

            throw new InvalidOperationException("Invalid VFS accepted: " + csv);
        }

        /// <summary>Учитывает проверку или прерывает тесты.</summary>
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
