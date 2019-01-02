using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Infrastructure.Logging;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace Services.TimeSlotLoaderSaver.GDrive
{
    public class GDriveTimeSlotLoaderSaver<T> : ITimeSlotBufferLoaderSaver<T>, IDisposable
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        private static readonly string[] Scopes = { DriveService.Scope.DriveFile };
        private const string ApplicationName = "CaldaiaBackend";
        
        private readonly ILogger _log;
        private DriveService _service;

        private const string RelCredentialPath = "GoogleDrive\\credentials.json";
        private const string RelTokenPath = "GoogleDrive\\token.json";
        private const string GFileMimeType = "application/json";

        private string BasePath { get; } = AppDomain.CurrentDomain.BaseDirectory;

        private readonly string _credentialsPath;
        private readonly string _tokenPath;
        private readonly string _fileName;

        private string _fileId;
        private string _toBeSaved;
        private Timer saveTimer;

        public GDriveTimeSlotLoaderSaver(
            string fileName,
            ILoggerFactory loggerFactory
            )
        {
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
            _credentialsPath = Path.Combine(BasePath, RelCredentialPath);
            _tokenPath = Path.Combine(BasePath, RelTokenPath);
            _fileName = fileName;
            DoLogin();
            ObtainFileId();
            StartSavingTimer();
        }

        private void StartSavingTimer()
        {
            saveTimer = new Timer(state => SaveToGDrive(), null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }

        private void ObtainFileId()
        {
            var findFileRequest = _service.Files.List();
            findFileRequest.Q = $"name = '{_fileName}' and mimeType = 'application/json'";
            var list = findFileRequest.Execute();

            var file = list.Files.FirstOrDefault();
            if (file == null)
            {
                file = new GoogleFile
                {
                    MimeType = GFileMimeType,
                    Name = _fileName
                };
                // The file does not exist and must be created.
                var createFileRequest = _service.Files.Create(file);

                file = createFileRequest.Execute();

                _log.Info($"Created file {_fileName} into google drive.", file);
            }
            _fileId = file.Id;
            _log.Info($"FileId for {_fileName} into google drive is {_fileId}.");
        }

        private void DoLogin()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_tokenPath, true)
                    ).Result;
                _log.Info("Credential file saved to: " + _tokenPath);
            }

            // Create Drive API service.
            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

        }

        public CircularTimeSlotBuffer<T> Load()
        {
            var fileRequest = _service.Files.Get(_fileId);
            using (var ms = new MemoryStream())
            {
                fileRequest.Download(ms);
                ms.Position = 0;
                var sr = new StreamReader(ms);
                var fileContent = sr.ReadToEnd();
                try
                {
                    var toReturn = CircularTimeSlotBuffer<T>.FromJson(fileContent);
                    return toReturn;
                }
                catch (Exception ex)
                {
                    _log.Warning($"Unable to create {nameof(CircularTimeSlotBuffer<T>)} from Google Drive file [{_fileName}/{_fileId}] content", ex, fileContent);
                    return null;
                }
            }
        }

        public void Save(CircularTimeSlotBuffer<T> toSave)
        {
            _toBeSaved = toSave.AsJson();
        }

        public void SaveToGDrive()
        {
            using (var toUpload = StringToStream(_toBeSaved))
            {
                var file = new GoogleFile();
                // var updateRequest = _service.Files.Create(file, toUpload, GFileMimeType);
                var updateRequest = _service.Files.Update(file, _fileId, toUpload, GFileMimeType);
                try
                {
                    updateRequest.Upload();
                    _log.Info($"Saved file to Google Drive [{_fileName}/{_fileId}].");
                }
                catch (Exception e)
                {
                    _log.Warning($"Unable to save {_fileName} to Google Drive", e);
                }

            }
        }

        private Stream StringToStream(string toStreamify)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, new UnicodeEncoding());
            sw.Write(toStreamify);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Dispose()
        {
            SaveToGDrive();
            _service?.Dispose();
            saveTimer?.Dispose();
        }
    }
}
