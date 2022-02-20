using System;
using System.IO;

namespace SimpleFileTransformer
{
    public sealed class FileWriter : IFileWriter, IDisposable
    {
        private readonly FileStream _file;

        public FileWriter(string filePath)
        {
            _file = File.Open(filePath, FileMode.Create);
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