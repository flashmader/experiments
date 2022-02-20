namespace SimpleFileTransformer.GZipTest
{
    internal struct Inputs
    {
        public TransformationType Transformation { get; set; }
        public string SourceFile { get; set; }
        public string TargetFile { get; set; }

        internal enum TransformationType
        {
            Compress,
            Decompress
        }
    }
}