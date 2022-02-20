using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleFileTransformer
{
    public sealed class FileReader : IFileReader, IDisposable
    {
        private readonly FileStream _fileStream;
        private static int _chunkSize;

        private FileReader(FileStream fileStream, int chunkSize)
        {
            _chunkSize = chunkSize;
            _fileStream = fileStream;
        }

        public static FileReader Open(string filePath, int chunkSize = 16_777_216)
        {
            try
            {
                var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                return new FileReader(fileStream, chunkSize);
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

        public IEnumerable<FileChunk> ReadChunks()
        {
            var buffer = new byte[_chunkSize];
            var chunkIndex = 0;

            while (true)
            {
                var bytesRead = _fileStream.Read(buffer, 0, _chunkSize);
                if (bytesRead == 0)
                {
                    break;
                }

                var chunkData = new byte[bytesRead];
                Array.Copy(buffer, chunkData, bytesRead);

                var chunk = new FileChunk(chunkData, chunkIndex++);
                yield return chunk;
            }
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}