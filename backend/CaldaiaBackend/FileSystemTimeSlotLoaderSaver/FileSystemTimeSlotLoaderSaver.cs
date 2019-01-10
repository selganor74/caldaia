using System;
using System.IO;
using System.Threading;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Infrastructure;
using Infrastructure.Logging;

namespace Application.Services
{
    public class FileSystemTimeSlotLoaderSaver<T> : ITimeSlotBufferLoaderSaver<T>, IDisposable
    {
        private readonly string _pathToJsonStorage;
        private Timer saveTimer;

        private string _toSave;
        private object _lock = new object();
        private readonly ILogger _log;

        public FileSystemTimeSlotLoaderSaver(
            string PathToJsonStorageFile,
            ILoggerFactory loggerFactory
        )
        {
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
            if (String.IsNullOrEmpty(PathToJsonStorageFile))
                throw new ArgumentException(nameof(PathToJsonStorageFile) +  " must be a valid path.");

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

            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }

        private void StartSavingTimer()
        {
            saveTimer = new Timer(state => SaveToFile(), null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }

        private void SaveToFile()
        {
            try
            {
                lock (_lock)
                {
                    _log.Info($"Writing data to {_pathToJsonStorage}");
                    File.WriteAllText(_pathToJsonStorage, _toSave);
                }
            }
            catch (Exception e)
            {
                _log.Warning($"Errors while writing file {_pathToJsonStorage}", e);
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
