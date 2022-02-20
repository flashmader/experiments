using System;

namespace SimpleFileTransformer.GZip
{
    public class CompressionException : Exception
    {
        public CompressionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}