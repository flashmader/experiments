using System.Collections.Generic;

namespace SimpleFileCompressor
{
    public interface IFileReader
    {
        IEnumerable<FileChunk> ReadChunks();
    }
}