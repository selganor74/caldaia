using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Infrastructure;
using Newtonsoft.Json;

namespace Application.Services
{
    public class FileSystemTimeSlotLoaderSaver<T> : ITimeSlotBufferLoaderSaver<T>
    {
        private readonly string _pathToJsonStorage;

        public FileSystemTimeSlotLoaderSaver(
            string PathToJsonStorageFile
        )
        {
            if (String.IsNullOrEmpty(PathToJsonStorageFile))
                throw new ArgumentException(nameof(PathToJsonStorageFile) +  " must be a valid path.");

            var currentExeDir = AppDomain.CurrentDomain.BaseDirectory;
            _pathToJsonStorage = PathToJsonStorageFile;

            if (_pathToJsonStorage.StartsWith("~"))
            {
                _pathToJsonStorage = _pathToJsonStorage.Replace("~\\", "");
                _pathToJsonStorage = Path.Combine(currentExeDir, _pathToJsonStorage);
            }

            if (!File.Exists(_pathToJsonStorage))
                Directory.CreateDirectory(Path.GetDirectoryName(_pathToJsonStorage));
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
            File.WriteAllText(_pathToJsonStorage, toSave.AsJson());
        }
    }
}
