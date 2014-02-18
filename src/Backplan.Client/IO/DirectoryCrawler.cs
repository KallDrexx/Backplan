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
            IEnumerable<string> filesInDirectory = _pathDetails.GetFilesInPath(path);
            var trackedFiles = _trackedFileStore.GetTrackedFilesInPath(path) ?? new TrackedFile[0];
            var processedTrackedFileNames = new List<string>();

            foreach (var trackedFile in trackedFiles)
            {
                var lastAction = trackedFile.Actions
                                            .OrderByDescending(x => x.EffectiveDateUtc)
                                            .First();

                var filePath = Path.Combine(lastAction.Path, lastAction.FileName);
                var fileInfo = _pathDetails.GetFileInfo(filePath);
                processedTrackedFileNames.Add(lastAction.FileName);

                // Check if the file was modified at all
                if (FileWasModified(lastAction,fileInfo))
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

            foreach (var filename in filesInDirectory)
            {
                if (_pathDetails.IsDirectory(filename))
                { 
                    CrawlDirectory(filename); 
                }
                else
                {
                    // Check if this is a new, non-tracked file
                    if (!processedTrackedFileNames.Any(x => x.Equals(filename, StringComparison.OrdinalIgnoreCase)))
                    {
                        var filePath = Path.Combine(path, filename);
                        var fileInfo = _pathDetails.GetFileInfo(filePath);
                        var action = new TrackedFileAction
                        {
                            Action = FileActions.Added,
                            Path = fileInfo.DirectoryName,
                            FileName = fileInfo.Name,
                            FileLength = fileInfo.Length,
                            FileLastModifiedDateUtc = fileInfo.LastWriteTimeUtc.DateTimeInstance,
                            EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                        };

                        _trackedFileStore.AddFileActionToTrackedFile(null, action);
                    }
                }
            }
        }

        private bool FileWasModified(TrackedFileAction lastAction, IFileInfoWrap fileInfo)
        {
            if (lastAction.FileLength != fileInfo.Length)
                return true;

            if (lastAction.FileLastModifiedDateUtc < fileInfo.LastWriteTimeUtc.DateTimeInstance)
                return true;

            return false;
        }
    }
}
