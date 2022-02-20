using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SimpleFileTransformer
{
    public sealed class FileTransformer
    {
        private readonly IFileReader _sourceFileReader;
        private readonly ITransformer _transformer;
        private readonly IFileWriter _resultWriter;
        
        private readonly int _chunksQueueSize = 32;
        private readonly BlockingCollection<FileChunk> _readFileChunks;

        private readonly AutoResetEvent _workersAutoReset;
        private int _lastTransformedChunkNumber = -1;
        private Thread[] _transformationWorkers;

        public FileTransformer(IFileReader sourceFileReader, ITransformer transformer, IFileWriter resultWriter)
        {
            _sourceFileReader = sourceFileReader;
            _transformer = transformer;
            _resultWriter = resultWriter;

            _readFileChunks = new BlockingCollection<FileChunk>(_chunksQueueSize);
            _workersAutoReset = new AutoResetEvent(true);
        }

        public void Transform()
        {
            StartTransformationWorkers();

            foreach (var fileChunk in _sourceFileReader.ReadChunks())
            {
                _readFileChunks.Add(fileChunk);
            }

            _readFileChunks.CompleteAdding();

            foreach (var transformationWorker in _transformationWorkers)
            {
                transformationWorker.Join();
            }
        }

        private void StartTransformationWorkers()
        {
            var threadsCount = Math.Max(Environment.ProcessorCount - 1, 1);
            _transformationWorkers = new Thread[threadsCount];

            for (var i = 0; i < threadsCount; i++)
            {
                var chunkProcessingWorker = new Thread(TransformChunk) { Name = $"Chunk processing worker N{i + 1}" };
                _transformationWorkers[i] = chunkProcessingWorker;
                chunkProcessingWorker.Start();
            }
        }

        private void TransformChunk()
        {
            while (!_readFileChunks.IsAddingCompleted || _readFileChunks.Count != 0)
            {
                var chunkTaken = _readFileChunks.TryTake(out var chunk);
                if (!chunkTaken)
                {
                    continue;
                }

                var transformationResult = _transformer.Transform(chunk);

                while (_workersAutoReset.WaitOne())
                {
                    if (_lastTransformedChunkNumber + 1 == chunk.Number)
                    {
                        break;
                    }

                    _workersAutoReset.Set();
                }

                _resultWriter.Write(transformationResult);

                _lastTransformedChunkNumber++;

                _workersAutoReset.Set();
            }
        }
    }


}