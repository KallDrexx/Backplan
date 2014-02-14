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
using Backplan.Client.Models;
using SystemWrapper;

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
            _mockContainer.Arrange<IPathDetails>(x => x.GetFilesInPath("C:\\Test"))
                          .Returns(new string[] { "C:\\Test\\Directory" });

            _mockContainer.Arrange<IPathDetails>(x => x.IsDirectory("C:\\Test\\Directory"))
                          .Returns(true);

            _mockContainer.Arrange<ITrackedFileStore>(x => x.GetTrackedFilesInPath("C:\\Test\\Directory"))
                          .OccursOnce();

            var instance = _mockContainer.Instance;
            instance.CheckDirectoryContents("C:\\Test");

            _mockContainer.AssertAll();
        }

        [TestMethod]
        public void No_Action_Added_When_Existing_File_Not_Changed()
        {
            const string filePath = @"C:\Test";
            const string fileName = "abc.def";
            const int fileLength = 100;
            DateTime writeTime = DateTime.Now.ToUniversalTime();

            var trackedFile = new TrackedFile
            {
                Actions = new[] 
                {
                    new TrackedFileAction 
                    {
                        Path = filePath,
                        FileName = fileName,
                        Action = FileActions.Added,
                        FileLength = fileLength,
                        FileLastModifiedDateUtc = writeTime,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    }
                }
            };

            var fileInfo = Mock.Create<IFileInfoWrap>();
            Mock.Arrange(() => fileInfo.Length).Returns(fileLength);
            Mock.Arrange(() => fileInfo.LastWriteTimeUtc).Returns(new DateTimeWrap(writeTime));
            Mock.Arrange(() => fileInfo.Name).Returns(fileName);
            Mock.Arrange(() => fileInfo.DirectoryName).Returns(filePath);

            _mockContainer.Arrange<ITrackedFileStore>(x => x.GetTrackedFilesInPath(filePath))
                          .Returns(new[] { trackedFile });

            _mockContainer.Arrange<IPathDetails>(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                          .Returns(fileInfo);

            _mockContainer.Arrange<ITrackedFileStore>(x => x.AddFileActionToTrackedFile(Arg.IsAny<TrackedFile>(), Arg.IsAny<TrackedFileAction>()))
                          .OccursNever();

            var instance = _mockContainer.Instance;
            instance.CheckDirectoryContents(filePath);

            _mockContainer.Assert<ITrackedFileStore>(x => x.AddFileActionToTrackedFile(Arg.IsAny<TrackedFile>(), Arg.IsAny<TrackedFileAction>()));
        }

        [TestMethod]
        public void Action_Added_When_File_Size_Doesnt_Match()
        {
            const string filePath = @"C:\Test";
            const string fileName = "abc.def";
            const int fileLength = 100;
            DateTime writeTime = DateTime.Now.ToUniversalTime();

            var trackedFile = new TrackedFile
            {
                Actions = new[] 
                {
                    new TrackedFileAction 
                    {
                        Path = filePath,
                        FileName = fileName,
                        Action = FileActions.Added,
                        FileLength = fileLength,
                        FileLastModifiedDateUtc = writeTime,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    }
                }
            };

            var fileInfo = Mock.Create<IFileInfoWrap>();
            Mock.Arrange(() => fileInfo.Length).Returns(fileLength + 1);
            Mock.Arrange(() => fileInfo.LastWriteTimeUtc).Returns(new DateTimeWrap(writeTime));
            Mock.Arrange(() => fileInfo.Name).Returns(fileName);
            Mock.Arrange(() => fileInfo.DirectoryName).Returns(filePath);

            _mockContainer.Arrange<ITrackedFileStore>(x => x.GetTrackedFilesInPath(filePath))
                          .Returns(new[] { trackedFile });

            _mockContainer.Arrange<IPathDetails>(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                          .Returns(fileInfo);

            var expectedAction = Arg.Matches<TrackedFileAction>(x => x.Action == FileActions.Modified &&
                                                                    x.FileLength == fileLength + 1);

            _mockContainer.Arrange<ITrackedFileStore>(x => x.AddFileActionToTrackedFile(Arg.IsAny<TrackedFile>(), expectedAction))
                          .OccursOnce();

            var instance = _mockContainer.Instance;
            instance.CheckDirectoryContents(filePath);

            _mockContainer.Assert<ITrackedFileStore>(x => x.AddFileActionToTrackedFile(Arg.IsAny<TrackedFile>(), Arg.IsAny<TrackedFileAction>()));
        }
    }
}
