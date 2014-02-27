using Backplan.Client.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Tests.Database
{
    [TestClass]
    public class InMemoryTrackedFileStoreTests : TrackedFileStoreBaseTests
    {
        [TestInitialize]
        public void InitStore()
        {
            _trackedFileStore = new InMemoryTrackedFileStore();
        }
    }
}
