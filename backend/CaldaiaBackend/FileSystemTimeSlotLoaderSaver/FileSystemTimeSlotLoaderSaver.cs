using System;
using System.IO;
using System.Threading;

using Infrastructure.Logging;

using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Infrastructure;

namespace Application.Services
{
    public class FileSystemTimeSlotLoaderSaver<T> : ITimeSlotBufferLoaderSaver<T>, IDisposable
    {
        private readonly string _pathToJsonStorage;
        private Timer saveTimer;

        private string _toSave;
        private object _lock = new object();
        private readonly ILogger _log;
        private readonly TimeSpan _saveInterval;

        public FileSystemTimeSlotLoaderSaver(
            string PathToJsonStorageFile,
            TimeSpan saveInterval,
            ILoggerFactory loggerFactory
        )
        {
            _log = loggerFactory?.CreateNewLogger($"{nameof(FileSystemTimeSlotLoaderSaver<T>)}<{typeof(T).Name}>") ?? new NullLogger();
            _saveInterval = saveInterval;
            if (String.IsNullOrEmpty(PathToJsonStorageFile))
                throw new ArgumentException(nameof(PathToJsonStorageFile) + " must be a valid path.");

            var currentExeDir = AppDomain.CurrentDomain.BaseDirectory;
            _pathToJsonStorage = PathToJsonStorageFile;

            if (_pathToJsonStorage.StartsWith("~"))
            {
                _pathToJsonStorage = _pathToJsonStorage.Replace("~\\", "").Replace("~", "");
                _pathToJsonStorage = Path.Combine(currentExeDir, _pathToJsonStorage);
                _log.Trace($"Resolved Path {PathToJsonStorageFile} to {_pathToJsonStorage}");
            }

            if (!File.Exists(_pathToJsonStorage))
                Directory.CreateDirectory(Path.GetDirectoryName(_pathToJsonStorage));

            StartSavingTimer();
        }

        private void StartSavingTimer()
        {
            _log.Info($"Saving to {_pathToJsonStorage} every {_saveInterval.TotalMinutes} minutes.");
            saveTimer = new Timer(state => SaveToFile(), null, _saveInterval, _saveInterval);
        }

        private void SaveToFile()
        {
            lock (_lock)
            {
                try
                {
                    _log.Info($"Writing data to {_pathToJsonStorage}", _toSave);
                    File.WriteAllText(_pathToJsonStorage, _toSave);
                }
                catch (Exception e)
                {
                    _log.Warning($"Errors while writing file {_pathToJsonStorage}", e, _toSave);
                }
            }

        }

        public CircularTimeSlotBuffer<T> Load()
        {
            try
            {
                var toDeserialize = File.ReadAllText(_pathToJsonStorage);

                return CircularTimeSlotBuffer<T>.FromJson(toDeserialize);
            }
            catch
            {
                return null;
            }
        }

        public void Save(CircularTimeSlotBuffer<T> toSave)
        {
            lock (_lock)
            {
                _toSave = toSave.AsJson();
            }
        }

        public void Dispose()
        {
            saveTimer?.Dispose();
            SaveToFile();
        }
    }
}
