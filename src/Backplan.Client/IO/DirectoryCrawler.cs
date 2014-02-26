using System.IO.Abstractions;
using Backplan.Client.Database;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backplan.Client.Models;

namespace Backplan.Client.IO
{
    public class DirectoryCrawler
    {
        private readonly IFileSystem _fileSystem;
        private readonly ITrackedFileStore _trackedFileStore;

        public DirectoryCrawler(ITrackedFileStore trackedFileStore, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _trackedFileStore = trackedFileStore;
        }

        public void CheckDirectoryContents(string baseDirectory)
        {
            CrawlDirectory(baseDirectory);
        }

        private void CrawlDirectory(string path)
        {
            IEnumerable<string> filesInDirectory = _fileSystem.Directory.GetFiles(path);
            IEnumerable<string> subfolders = _fileSystem.Directory.GetDirectories(path);
            var trackedFiles = _trackedFileStore.GetTrackedFilesInPath(path) ?? new TrackedFile[0];
            var processedTrackedFileNames = new List<string>();

            foreach (var trackedFile in trackedFiles)
            {
                var lastAction = trackedFile.Actions
                                            .OrderByDescending(x => x.EffectiveDateUtc)
                                            .First();

                var filePath = Path.Combine(lastAction.Path, lastAction.FileName);
                var fileInfo = _fileSystem.FileInfo.FromFileName(filePath);
                processedTrackedFileNames.Add(filePath);

                // Check if the file was modified at all
                if (!fileInfo.Exists)
                {
                    var newAction = new TrackedFileAction
                    {
                        Action = FileActions.Deleted,
                        Path = path,
                        FileName = lastAction.FileName,
                        FileLength = 0,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime(),
                        FileLastModifiedDateUtc = DateTime.Now.ToUniversalTime()
                    };

                    _trackedFileStore.AddFileActionToTrackedFile(trackedFile, newAction);
                }
                else if (FileWasModified(lastAction,fileInfo))
                {
                    var newAction = new TrackedFileAction
                    {
                        Action = FileActions.Modified,
                        Path = fileInfo.DirectoryName,
                        FileName = fileInfo.Name,
                        FileLength = fileInfo.Length,
                        FileLastModifiedDateUtc = fileInfo.LastWriteTimeUtc,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    };

                    _trackedFileStore.AddFileActionToTrackedFile(trackedFile, newAction);
                }
            }

            foreach (var filename in filesInDirectory)
            {
                var filePath = Path.Combine(path, filename);
                var fileInfo = _fileSystem.FileInfo.FromFileName(filePath);

                // Check if this is a new, non-tracked file
                if (!processedTrackedFileNames.Any(x => x.Equals(filename, StringComparison.OrdinalIgnoreCase)))
                {
                    var action = new TrackedFileAction
                    {
                        Action = FileActions.Added,
                        Path = fileInfo.DirectoryName,
                        FileName = fileInfo.Name,
                        FileLength = fileInfo.Length,
                        FileLastModifiedDateUtc = fileInfo.LastWriteTimeUtc,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    };

                    _trackedFileStore.AddFileActionToTrackedFile(null, action);
                }
            }

            foreach (var subfolder in subfolders)
            {
                CrawlDirectory(subfolder);
            }
        }

        private bool FileWasModified(TrackedFileAction lastAction, FileInfoBase fileInfo)
        {
            if (lastAction.FileLength != fileInfo.Length)
                return true;

            if (lastAction.FileLastModifiedDateUtc < fileInfo.LastWriteTimeUtc)
                return true;

            return false;
        }

        private bool IsDirectory(FileInfoBase fileInfo)
        {
            FileAttributes attributes = fileInfo.Attributes;
            return ((attributes & FileAttributes.Directory) == FileAttributes.Directory);
        }
    }
}
