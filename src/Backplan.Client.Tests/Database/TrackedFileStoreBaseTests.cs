using System.IO;
using Backplan.Client.Database;
using Backplan.Client.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Tests.Database
{
    [Ignore]
    public abstract class TrackedFileStoreBaseTests
    {
        protected ITrackedFileStore _trackedFileStore;

        [TestMethod]
        public void Can_Add_And_Retrieve_Tracked_File_By_Path()
        {
            const string filename = "abc.def";
            const string directory = @"C:\\temp";
            string fullPath = Path.Combine(directory, filename);
            
            _trackedFileStore.AddFileActionToTrackedFile(null, new TrackedFileAction
            {
                FileName = filename,
                Path = directory
            });

            var result = _trackedFileStore.GetTrackedFileByFullPath(fullPath);

            Assert.IsNotNull(result, "Null tracked file returned");
            Assert.IsNotNull(result.Actions, "Tracked file had null actions enumerable");
            Assert.AreEqual(1, result.Actions.Count(), "Tracked file had incorrect number of actions");
            Assert.AreEqual(filename, result.Actions.First().FileName, "Tracked file action had incorrect file name");
            Assert.AreEqual(directory, result.Actions.First().Path, "Tracked file action had incorrect path");
        }
    }
}
