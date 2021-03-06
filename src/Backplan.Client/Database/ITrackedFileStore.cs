﻿using Backplan.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Database
{
    public interface ITrackedFileStore
    {
        IEnumerable<TrackedFile> GetTrackedFilesInPath(string path);
        void AddFileActionToTrackedFile(TrackedFile file, TrackedFileAction action);
        TrackedFile GetTrackedFileByFullPath(string nameWithPath);
    }
}
