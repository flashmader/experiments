using System;
using System.Collections.Concurrent;
using SimpleFileCompressor;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var input = Console.ReadLine();

            if (input == "1")
            {
                var transformer = new GZipCompressor();
                IFileReader srcFile = new FileReader("big.bmp");
                using var targetFile = new FileWriter("compressed");

                var fileTransformer = new FileTransformer(srcFile, transformer, targetFile);

                fileTransformer.Transform();
            }

            else if (input == "2")
            {
                var transformer = new GZipDecompressor();
                IFileReader srcFile = new FileReader("compressed");
                using var targetFile = new FileWriter("decompressed.bmp");

                var fileTransformer = new FileTransformer(srcFile, transformer, targetFile);

                fileTransformer.Transform();
            }

            Console.WriteLine("Hello World!");
        }
    }
}
