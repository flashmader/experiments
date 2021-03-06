using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private int _lastTransformedChunkNumber = -1;

        private readonly Thread[] _transformationWorkers;
        private readonly AutoResetEvent _workersAutoReset;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly LinkedList<Exception> exceptions = new LinkedList<Exception>();
        private readonly object _exceptionHandlerLock = new object();

        public IEnumerable<Exception> Exceptions => exceptions;

        public FileTransformer(IFileReader sourceFileReader, ITransformer transformer, IFileWriter resultWriter)
        {
            _sourceFileReader = sourceFileReader;
            _transformer = transformer;
            _resultWriter = resultWriter;

            _readFileChunks = new BlockingCollection<FileChunk>(_chunksQueueSize);
            _workersAutoReset = new AutoResetEvent(true);

            var threadsCount = Math.Max(Environment.ProcessorCount - 1, 1);
            _transformationWorkers = new Thread[threadsCount];
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Transform()
        {
            StartTransformationWorkers();

            try
            {
                foreach (var fileChunk in _sourceFileReader.ReadChunks())
                {
                    _readFileChunks.Add(fileChunk, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                exceptions.AddLast(new Exception("Reading source file was cancelled."));
            }

            _readFileChunks.CompleteAdding();

            foreach (var transformationWorker in _transformationWorkers)
            {
                transformationWorker.Join();
            }
        }

        private void StartTransformationWorkers()
        {
            void ExceptionHandler(Exception ex)
            {
                lock (_exceptionHandlerLock)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                        exceptions.AddFirst(ex);
                    }
                }
            }

            for (var i = 0; i < _transformationWorkers.Length; i++)
            {
                var chunkProcessingWorker = new Thread(() => TransformChunk(ExceptionHandler, _cancellationTokenSource.Token))
                {
                    Name = $"Chunk processing worker N{i + 1}"
                };
                _transformationWorkers[i] = chunkProcessingWorker;
                chunkProcessingWorker.Start();
            }
        }

        private void TransformChunk(Action<Exception> exceptionHandler, CancellationToken cancellation)
        {
            try
            {
                while (!_readFileChunks.IsAddingCompleted || _readFileChunks.Count != 0)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }

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
            catch (Exception e)
            {
                exceptionHandler(e);
                _workersAutoReset.Set();
            }
        }
    }
}