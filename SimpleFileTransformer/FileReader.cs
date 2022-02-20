using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleFileTransformer
{
    public sealed class FileReader : IFileReader
    {
        private readonly string _filePath;
        private readonly int _chunkSize = 16_777_216;

        public FileReader(string filePath)
        {
            _filePath = filePath;
        }

        public IEnumerable<FileChunk> ReadChunks()
        {
            var buffer = new byte[_chunkSize];
            using var inputStream = File.Open(_filePath, FileMode.Open, FileAccess.Read);

            var chunkIndex = 0;

            while (true)
            {
                var bytesRead = inputStream.Read(buffer, 0, _chunkSize);
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
    }
}