namespace SimpleFileCompressor
{
    public readonly struct FileChunk
    {
        public FileChunk(byte[] data, int number)
        {
            Data = data;
            Number = number;
        }

        public byte[] Data { get; }
        public int Number { get; }
        public int Length => Data.Length;
    }
}