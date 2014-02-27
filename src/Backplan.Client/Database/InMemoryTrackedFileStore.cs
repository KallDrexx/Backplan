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
            throw new NotImplementedException();
        }

        public void AddFileActionToTrackedFile(TrackedFile file, TrackedFileAction action)
        {
            file = new TrackedFile();
            file.Actions.Add(action);

            _trackedFiles.Add(file);
        }

        public TrackedFile GetTrackedFileByFullPath(string nameWithPath)
        {
            return _trackedFiles.First(x => x.Actions
                                             .Any(y => Path.Combine(y.Path, y.FileName) == nameWithPath));
        }
    }
}
