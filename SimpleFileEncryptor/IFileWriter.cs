using System;

namespace SimpleFileCompressor
{
    public interface IFileWriter
    {
        void Write(byte[] bytes);
    }
}