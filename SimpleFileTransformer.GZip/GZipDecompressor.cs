using System.IO;
using System.IO.Compression;

namespace SimpleFileTransformer.GZip
{
    public class GZipDecompressor : ITransformer
    {
        public byte[] Transform(FileChunk chunk)
        {
            using var zippedStream = new MemoryStream(chunk.Data);
            using var gZipStream = new GZipStream(zippedStream, CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();

            gZipStream.CopyTo(decompressedStream);
            return decompressedStream.ToArray();
        }
    }
}