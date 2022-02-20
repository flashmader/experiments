using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using SimpleFileTransformer.GZip;

namespace SimpleFileTransformer.GZipTest
{
    internal static class ConsoleHelper
    {
        public static void WriteRed(string message)
        {
            WriteColored(message, ConsoleColor.DarkRed);
        }

        public static void WriteGreen(string message)
        {
            WriteColored(message, ConsoleColor.DarkGreen);
        }

        public static void WriteYellow(string message)
        {
            WriteColored(message, ConsoleColor.DarkYellow);
        }

        private static void WriteColored(string message, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message);

            Console.ForegroundColor = originalColor;
        }
    }

    internal class Program
    {
        private const int Failure = 0;
        private const int Success = 1;

        static int Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var inputs = ReadInputArguments(args);
                if (inputs == null)
                {
                    return Failure;
                }

                var fileReader = InitializeReader(inputs.Value);
                if (fileReader == null)
                {
                    return Failure;
                }

                var fileWriter = InitializeWriter(inputs.Value);
                if (fileWriter == null)
                {
                    return Failure;
                }

                var fileTransformation = InitializeTransformer(inputs.Value);
                var fileTransformer = new FileTransformer(fileReader, fileTransformation, fileWriter);

                Console.WriteLine("File processing started...");
                fileTransformer.Transform();

                if (!fileTransformer.Exceptions.Any())
                {
                    ConsoleHelper.WriteGreen("File processing finished successfully.");
                    return Success;
                }
                else
                {
                    ConsoleHelper.WriteRed("File processing failed.");
                    foreach (var exception in fileTransformer.Exceptions)
                    {
                        ConsoleHelper.WriteYellow($"     {exception.Message}");
                    }
                    return Failure;
                }
            }
            catch (CompressionException ex)
            {
                ConsoleHelper.WriteRed($"A problem occurred during GZip compression: {ex.Message}");
                return Failure;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteRed($"A problem occurred: {ex.Message}");
                return Failure;
            }
            finally
            {
                stopwatch.Stop();

                #if DEBUG
                ConsoleHelper.WriteGreen("Time spent: " + stopwatch.Elapsed);
                #endif 
            }
        }

        private static ITransformer InitializeTransformer(Inputs inputs)
        {
            return inputs.Transformation == Inputs.TransformationType.Compress
                ? (ITransformer) new GZipCompressor()
                : new GZipDecompressor();
        }

        private static FileReader InitializeReader(Inputs inputs)
        {
            try
            {
                return FileReader.Open(inputs.SourceFile);
            }
            catch (FileHandlingException ex)
            {
                ConsoleHelper.WriteRed("A problem occurred while opening the source file for reading:");
                ConsoleHelper.WriteYellow(ex.Message);
                
                return null;
            }
        }

        private static FileWriter InitializeWriter(Inputs inputs)
        {
            try
            {
                return FileWriter.Open(inputs.TargetFile);
            }
            catch (FileHandlingException ex)
            {
                ConsoleHelper.WriteRed("A problem occurred while creating the target file:");
                ConsoleHelper.WriteYellow(ex.Message);

                return null;
            }
        }

        private static Inputs? ReadInputArguments(string[] args)
        {
            var inputs = new Inputs();

            #if DEBUG

            ConsoleHelper.WriteGreen("For compression enter 'c', any other for decompression");
            inputs.Transformation = Console.ReadLine() == "c"
                ? Inputs.TransformationType.Compress
                : Inputs.TransformationType.Decompress;

            ConsoleHelper.WriteGreen("Source file:");
            inputs.SourceFile = Console.ReadLine();

            ConsoleHelper.WriteGreen("Target file:");
            inputs.TargetFile = Console.ReadLine();
            Console.WriteLine();

            #else

            if (args.Length != 3)
            {
                var exeName = AppDomain.CurrentDomain.FriendlyName;
                ConsoleHelper.WriteRed($"{exeName}.exe must be called with exactly three arguments:");
                Console.WriteLine($"{exeName}.exe [compress | decompress] [source file name] [output file name]");
                return null;
            }
            
            if(args[0] != "compress" && args[0] != "decompress")
            {
                ConsoleHelper.WriteRed("Only 'compress' or 'decompress' are allowed as for the first argument");
                return null;
            }

            inputs = new Inputs
            {
                Transformation = args[0] == "compress" 
                    ? Inputs.TransformationType.Compress 
                    : Inputs.TransformationType.Decompress,
                SourceFile = args[1],
                TargetFile = args[2]
            };

            #endif

            return inputs;
        }
    }
}
