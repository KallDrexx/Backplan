using Backplan.Client.Database;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;
using Backplan.Client.Models;

namespace Backplan.Client.IO
{
    public class DirectoryCrawler
    {
        private IPathDetails _pathDetails;
        private ITrackedFileStore _trackedFileStore;

        public DirectoryCrawler(ITrackedFileStore trackedFileStore, IPathDetails pathDetails)
        {
            _pathDetails = pathDetails;
            _trackedFileStore = trackedFileStore;
        }

        public void CheckDirectoryContents(string baseDirectory)
        {
            CrawlDirectory(baseDirectory);
        }

        private void CrawlDirectory(string path)
        {
            var trackedFiles = _trackedFileStore.GetTrackedFilesInPath(path) ?? new TrackedFile[0];
            foreach (var trackedFile in trackedFiles)
            {
                var lastAction = trackedFile.Actions
                                            .OrderByDescending(x => x.EffectiveDateUtc)
                                            .First();

                var filePath = Path.Combine(lastAction.Path, lastAction.FileName);
                var fileInfo = _pathDetails.GetFileInfo(filePath);

                // Check if the file was modified at all
                if (lastAction.FileLength != fileInfo.Length)
                {
                    var newAction = new TrackedFileAction
                    {
                        Action = FileActions.Modified,
                        Path = fileInfo.DirectoryName,
                        FileName = fileInfo.Name,
                        FileLength = fileInfo.Length,
                        FileLastModifiedDateUtc = fileInfo.LastWriteTimeUtc.DateTimeInstance,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    };

                    _trackedFileStore.AddFileActionToTrackedFile(trackedFile, newAction);
                }
            }

            var filesInDirectory = _pathDetails.GetFilesInPath(path);
            foreach (var filename in filesInDirectory)
            {
                if (_pathDetails.IsDirectory(filename))
                    CrawlDirectory(filename);
            }
        }
    }
}
