using Backplan.Client.Database;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;

namespace Backplan.Client.IO
{
    public class DirectoryCrawler
    {
        private IPathWrap _path;
        private IDirectoryWrap _directory;
        private ITrackedFileStore _trackedFileStore;
        private IFileWrap _file;

        public DirectoryCrawler(IPathWrap path, ITrackedFileStore trackedFileStore, IDirectoryWrap directory, IFileWrap file)
        {
            _path = path;
            _trackedFileStore = trackedFileStore;
            _directory = directory;
            _file = file;
        }

        public void CheckDirectoryContents(string baseDirectory)
        {
            CrawlDirectory(baseDirectory);
        }

        private void CrawlDirectory(string path)
        {
            var trackedFiles = _trackedFileStore.GetTrackedFilesInPath(path);

            var filesInDirectory = _directory.GetFiles(path);
            foreach (var filename in filesInDirectory)
            {
                var attributes = _file.GetAttributes(filename);
                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    CrawlDirectory(filename);
                }
            }
        }
    }
}
