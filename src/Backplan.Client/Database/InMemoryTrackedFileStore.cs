using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backplan.Client.Models;

namespace Backplan.Client.Database
{
    public class InMemoryTrackedFileStore : ITrackedFileStore
    {
        private readonly List<TrackedFile> _trackedFiles;

        public InMemoryTrackedFileStore()
        {
            _trackedFiles = new List<TrackedFile>();
        }

        public IEnumerable<TrackedFile> GetTrackedFilesInPath(string path)
        {
            return _trackedFiles.Where(x => x.Actions.Any(y => y.Path == path))
                                .ToArray();
        }

        public void AddFileActionToTrackedFile(TrackedFile file, TrackedFileAction action)
        {
            if (file == null)
            {
                file = new TrackedFile();
                _trackedFiles.Add(file);
            }

            file.Actions.Add(action);
        }

        public TrackedFile GetTrackedFileByFullPath(string nameWithPath)
        {
            return _trackedFiles.FirstOrDefault(x => x.Actions
                                                      .Any(y => Path.Combine(y.Path, y.FileName) == nameWithPath));
        }
    }
}
