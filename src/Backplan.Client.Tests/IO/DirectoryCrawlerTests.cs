using Backplan.Client.Database;
using Backplan.Client.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;
using Telerik.JustMock;
using Telerik.JustMock.AutoMock;

namespace Backplan.Client.Tests.IO
{
    [TestClass]
    public class DirectoryCrawlerTests
    {
        private MockingContainer<DirectoryCrawler> _mockContainer;

        [TestInitialize]
        public void Setup()
        {
            _mockContainer = new MockingContainer<DirectoryCrawler>();
        }

        [TestMethod]
        public void Requests_Tracked_Files_In_Path()
        {
            _mockContainer.Arrange<ITrackedFileStore>(x => x.GetTrackedFilesInPath("C:\\Test"))
                          .OccursOnce();

            var instance = _mockContainer.Instance;
            instance.CheckDirectoryContents("C:\\Test");

            _mockContainer.AssertAll();
        }

        [TestMethod]
        public void Requests_Tracked_Files_In_Subfolder()
        {
            _mockContainer.Arrange<IDirectoryWrap>(x => x.GetFiles("C:\\Test"))
                          .Returns(new string[] { "C:\\Test\\Directory" });

            _mockContainer.Arrange<IFileWrap>(x => x.GetAttributes("C:\\Test\\Directory"))
                          .Returns(FileAttributes.Directory);

            _mockContainer.Arrange<ITrackedFileStore>(x => x.GetTrackedFilesInPath("C:\\Test\\Directory"))
                          .OccursOnce();

            var instance = _mockContainer.Instance;
            instance.CheckDirectoryContents("C:\\Test");

            _mockContainer.AssertAll();
        }
    }
}
