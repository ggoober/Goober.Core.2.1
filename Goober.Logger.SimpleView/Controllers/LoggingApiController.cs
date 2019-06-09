using Goober.Logging.SimpleView.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Goober.BackgroundWorker.Controllers
{
    public class LoggingApiController : Controller
    {
        public const string LogsPathKey = "Goober.Logging.LogsPath";
        public const string LogsFilesTemplateKey = "Goober.Logging.FilesTemplate";
        public const string SecretKey = "Goober.Logging.Secret";

        private readonly IConfiguration _configuration;

        public LoggingApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/logging/get-files")]
        public List<GetFilesListResponse> GetFiles(string secret)
        {
            if (secret != _configuration[SecretKey])
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.NotFound);

            var files = GetFilesList();

            var ret = files.Select(ConvertToFileListResponse)
                .OrderBy(x => x.Directory)
                .ThenByDescending(x => x.LastChangedDate)
                .ToList();

            return ret;
        }

        [HttpGet]
        [Route("api/logging/get-file-logs")]
        public GetFileLogRecordsResponse GetFileLogRecords(string secret, 
            string fileNameWithExtension)
        {
            if (secret != _configuration[SecretKey])
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.NotFound);

            var files = GetFilesList();

            var currentFile = files.FirstOrDefault(x => (x.Name + x.Extension).ToLower() == fileNameWithExtension.Trim().ToLower());

            if (currentFile == null)
                throw new InvalidOperationException($"Can't find file {fileNameWithExtension}");

            return GetFileLogs(currentFile);
        }

        [HttpGet]
        [Route("api/logging/get-last-changed-file-logs")]
        public GetFileLogRecordsResponse GetLastChangedFileLogRecords(string secret)
        {
            if (secret != _configuration[SecretKey])
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.NotFound);

            var files = GetFilesList();

            var currentFile = files.OrderByDescending(x => x.LastWriteTime).FirstOrDefault();

            if (currentFile == null)
                return new GetFileLogRecordsResponse();

            return GetFileLogs(currentFile);
        }

        private FileInfo[] GetFilesList()
        {
            var fullPath = GetFullPath();

            var filesTemplate = _configuration[LogsFilesTemplateKey];

            var dirInfo = new DirectoryInfo(fullPath);

            var files = dirInfo.GetFiles(filesTemplate, SearchOption.AllDirectories);

            return files;
        }

        private static GetFileLogRecordsResponse GetFileLogs(FileInfo currentFile)
        {
            var ret = new GetFileLogRecordsResponse
            {
                FileFullPath = currentFile.FullName
            };

            var lines = System.IO.File.ReadAllLines(currentFile.FullName);
            lines = lines.Reverse().ToArray();

            var recordsAddedCount = 0;
            foreach (var iLine in lines)
            {
                var recordDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(iLine);
                if (recordDict == null)
                    continue;

                if (recordDict.Count == 0)
                    continue;

                ret.Records.Add(recordDict);
                recordsAddedCount++;
            }

            return ret;
        }

        private string GetFullPath()
        {
            var path = _configuration[LogsPathKey] ?? "logs";

            if (Path.IsPathRooted(path))
                return path;

            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var fullPath = Path.Combine(currentDirectory, path);

            return fullPath;
        }

        private GetFilesListResponse ConvertToFileListResponse(FileInfo fileInfo)
        {
            var ret = new GetFilesListResponse
            {
                FullPath = fileInfo.FullName,
                Directory = fileInfo.DirectoryName,
                LengthInBytes = fileInfo.Length,
                Name = fileInfo.Name,
                Extension = fileInfo.Extension,
                LastChangedDate = fileInfo.LastWriteTime,
                CreatedDate = fileInfo.CreationTime
            };

            return ret;
        }
    }
}
