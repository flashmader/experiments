using System.IO;
using System.IO.Compression;

namespace SimpleFileTransformer.GZip
{
    public sealed class GZipDecompressor : ITransformer
    {
        public byte[] Transform(FileChunk chunk)
        {
            try
            {
                using var zippedStream = new MemoryStream(chunk.Data);
                using var gZipStream = new GZipStream(zippedStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();

                gZipStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
            catch (InvalidDataException ex)
            {
                throw new CompressionException("The source file is corrupted or compressed with unknown algorithm.", ex);
            }
        }
    }
}