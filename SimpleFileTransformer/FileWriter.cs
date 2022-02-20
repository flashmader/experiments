using System;
using System.IO;

namespace SimpleFileTransformer
{
    public sealed class FileWriter : IFileWriter, IDisposable
    {
        private readonly FileStream _file;

        private FileWriter(FileStream stream)
        {
            _file = stream;
        }

        public static FileWriter Open(string filePath)
        {
            try
            {
                var fileStream = File.Open(filePath, FileMode.CreateNew);
                return new FileWriter(fileStream);
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is ArgumentNullException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is FileNotFoundException ||
                ex is NotSupportedException)
            {
                throw new FileHandlingException(ex.Message, ex);
            }
        }

        public void Write(byte[] bytes)
        {
            _file.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            _file.Dispose();
        }
    }
}