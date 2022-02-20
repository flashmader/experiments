using System.IO;
using System.IO.Compression;

namespace SimpleFileTransformer.GZip
{
    public class GZipCompressor : ITransformer
    {
        public byte[] Transform(FileChunk chunk)
        {
            using var zippedStream = new MemoryStream();
            using var gZipStream = new GZipStream(zippedStream, CompressionMode.Compress);

            gZipStream.Write(chunk.Data, 0, chunk.Length);
            gZipStream.Close();

            return zippedStream.ToArray();
        }
    }
}