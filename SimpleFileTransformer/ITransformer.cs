namespace SimpleFileTransformer
{
    public interface ITransformer
    {
        byte[] Transform(FileChunk chunk);
    }
}