using System;

namespace SimpleFileTransformer
{
    public class FileHandlingException : Exception
    {
        public FileHandlingException(string message, Exception inner) 
            : base(message, inner)
        {
        }
    }
}