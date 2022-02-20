namespace SimpleFileCompressor
{
    public interface ITransformer
    {
        byte[] Transform(FileChunk chunk);
    }
}