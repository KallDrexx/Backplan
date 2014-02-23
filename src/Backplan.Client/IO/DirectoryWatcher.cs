using Backplan.Client.Database;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.IO
{
    /// <summary>
    /// Watches a directory and notifies the tracked file store of changes
    /// </summary>
    public class DirectoryWatcher
    {
        private readonly FileSystemWatcherBase _fileSystemWatcher;

        public DirectoryWatcher(FileSystemWatcherBase fileSystemWatcher)
        {
            _fileSystemWatcher = fileSystemWatcher;
        }

        public void Begin(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("FileSystemWatcher passed in without a valid path already set");

            _fileSystemWatcher.Path = path;
        }
    }
}
