using System.IO;
using Backplan.Client.Database;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backplan.Client.Models;

namespace Backplan.Client.IO
{
    /// <summary>
    /// Watches a directory and notifies the tracked file store of changes
    /// </summary>
    public class DirectoryWatcher : IDisposable
    {
        private readonly FileSystemWatcherBase _fileSystemWatcher;
        private readonly ITrackedFileStore _trackedFileStore;
        private readonly IFileSystem _fileSystem;

        public DirectoryWatcher(FileSystemWatcherBase fileSystemWatcher, ITrackedFileStore trackedFileStore, IFileSystem fileSystem)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _trackedFileStore = trackedFileStore;
            _fileSystem = fileSystem;
        }

        public void Start(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("FileSystemWatcher passed in without a valid path already set");

            _fileSystemWatcher.Path = path;
            _fileSystemWatcher.Created += FileSystemWatcherOnCreatedOrChanged;
            _fileSystemWatcher.Changed += FileSystemWatcherOnCreatedOrChanged;
            _fileSystemWatcher.Deleted += FileSystemWatcherOnCreatedOrChanged;

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileSystemWatcherOnCreatedOrChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            FileActions action;
            TrackedFile trackedFile;

            switch (fileSystemEventArgs.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    action = FileActions.Added;
                    trackedFile = null;
                    break;

                case WatcherChangeTypes.Changed:
                    action = FileActions.Modified;
                    trackedFile = _trackedFileStore.GetTrackedFileByFullPath(fileSystemEventArgs.FullPath);
                    break;

                case WatcherChangeTypes.Deleted:
                    action = FileActions.Deleted;
                    trackedFile = _trackedFileStore.GetTrackedFileByFullPath(fileSystemEventArgs.FullPath);
                    break;

                default:
                    action = FileActions.None;
                    trackedFile = null;
                    break;
            }

            var fileInfo = _fileSystem.FileInfo.FromFileName(fileSystemEventArgs.FullPath);
            _trackedFileStore.AddFileActionToTrackedFile(trackedFile, new TrackedFileAction
            {
                Path = fileInfo.DirectoryName,
                FileName = fileInfo.Name,
                Action = action,
                EffectiveDateUtc = DateTime.Now.ToUniversalTime(),
                FileLength = fileInfo.Length,
                FileLastModifiedDateUtc = fileInfo.LastWriteTimeUtc
            });
        }

        public void Dispose()
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
        }
    }
}
