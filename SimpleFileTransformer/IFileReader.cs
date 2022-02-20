using System.Collections.Generic;

namespace SimpleFileTransformer
{
    public interface IFileReader
    {
        IEnumerable<FileChunk> ReadChunks();
    }
}