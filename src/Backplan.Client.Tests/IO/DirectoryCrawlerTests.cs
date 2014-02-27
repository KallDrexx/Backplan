using Backplan.Client.Database;
using Backplan.Client.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backplan.Client.Models;
using AutoMoq;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Backplan.Client.Tests.IO
{
    [TestClass]
    public class DirectoryCrawlerTests
    {
        private const string BaseDirectory = @"C:\Test";

        private AutoMoqer _mocker;
        private MockFileSystem _fileSystem;

        [TestInitialize]
        public void Setup()
        {
            _mocker = new AutoMoqer();

            // GetMock of the abstract class before create to prevent automoq bugs
            _mocker.GetMock<FileSystemWatcherBase>();

            _fileSystem = new MockFileSystem();
            _fileSystem.AddDirectory(BaseDirectory);
            _mocker.SetInstance<IFileSystem>(_fileSystem);
        }

        [TestMethod]
        public void Requests_Tracked_Files_In_Path()
        {
            _fileSystem.AddDirectory(BaseDirectory);

            var instance = _mocker.Create<DirectoryCrawler>();
            instance.CheckDirectoryContents(BaseDirectory);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.GetTrackedFilesInPath(BaseDirectory), Times.Once);
        }

        [TestMethod]
        public void Requests_Tracked_Files_In_Subfolder()
        {
            const string subFolder = @"C:\Test\Directory";

            _fileSystem.AddDirectory(subFolder);

            var instance = _mocker.Create<DirectoryCrawler>();
            instance.CheckDirectoryContents("C:\\Test");

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.GetTrackedFilesInPath(@"C:\Test\Directory"), Times.Once);
        }

        [TestMethod]
        public void No_Action_Added_When_Existing_File_Not_Changed()
        {
            DateTime writeTime = DateTime.Now.ToUniversalTime();
            var filePath = Path.Combine(BaseDirectory, "abc.def");
            var content = new byte[] {1, 1, 1};
            _fileSystem.AddFile(filePath,new MockFileData(content)
            {
                LastWriteTime = writeTime
            });

            var instance = _mocker.Create<DirectoryCrawler>();
            var trackedFile = new TrackedFile
            {
                Actions = new List<TrackedFileAction>(new[] 
                {
                    new TrackedFileAction 
                    {
                        Path = BaseDirectory,
                        FileName = Path.GetFileName(filePath),
                        Action = FileActions.Added,
                        FileLength = content.Length,
                        FileLastModifiedDateUtc = writeTime,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    }
                })
            };

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(BaseDirectory))
                   .Returns(new[] { trackedFile });

            instance.CheckDirectoryContents(BaseDirectory);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(It.IsAny<TrackedFile>(), It.IsAny<TrackedFileAction>()), Times.Never);
        }

        [TestMethod]
        public void Action_Added_When_File_Size_Doesnt_Match()
        {
            DateTime writeTime = DateTime.Now.ToUniversalTime();
            var filePath = Path.Combine(BaseDirectory, "abc.def");
            var content = new byte[] { 1, 1, 1 };
            _fileSystem.AddFile(filePath, new MockFileData(content)
            {
                LastWriteTime = writeTime
            });

            var instance = _mocker.Create<DirectoryCrawler>();

            var trackedFile = new TrackedFile
            {
                Actions = new List<TrackedFileAction>(new[] 
                {
                    new TrackedFileAction 
                    {
                        Path = BaseDirectory,
                        FileName = Path.GetFileName(filePath),
                        Action = FileActions.Added,
                        FileLength = content.Length - 1,
                        FileLastModifiedDateUtc = writeTime,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    }
                })
            };

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(BaseDirectory))
                   .Returns(new[] { trackedFile });

            instance.CheckDirectoryContents(BaseDirectory);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Modified)),
                            Times.Once);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.FileLength == content.Length)),
                            Times.Once);
        }

        [TestMethod]
        public void Action_Added_When_File_Modified_Date_Is_Newer()
        {
            DateTime writeTime = DateTime.Now.ToUniversalTime();
            var filePath = Path.Combine(BaseDirectory, "abc.def");
            var content = new byte[] { 1, 1, 1 };
            _fileSystem.AddFile(filePath, new MockFileData(content)
            {
                LastWriteTime = writeTime
            });

            var instance = _mocker.Create<DirectoryCrawler>();

            var trackedFile = new TrackedFile
            {
                Actions = new List<TrackedFileAction>(new[] 
                {
                    new TrackedFileAction 
                    {
                        Path = BaseDirectory,
                        FileName = Path.GetFileName(filePath),
                        Action = FileActions.Added,
                        FileLength = content.Length,
                        FileLastModifiedDateUtc = writeTime.AddDays(-1),
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    }
                })
            };

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(BaseDirectory))
                   .Returns(new[] { trackedFile });

            instance.CheckDirectoryContents(BaseDirectory);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Modified)),
                            Times.Once);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.FileLastModifiedDateUtc == writeTime)),
                            Times.Once);
        }

        [TestMethod]
        public void Action_Added_When_File_Isnt_In_Tracked_List()
        {
            DateTime writeTime = DateTime.Now.ToUniversalTime();
            var filePath = Path.Combine(BaseDirectory, "abc.def");
            var content = new byte[] { 1, 1, 1 };
            _fileSystem.AddFile(filePath, new MockFileData(content)
            {
                LastWriteTime = writeTime
            });

            var instance = _mocker.Create<DirectoryCrawler>();

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(filePath))
                   .Returns(new TrackedFile[] { });

            instance.CheckDirectoryContents(BaseDirectory);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(null, It.Is<TrackedFileAction>(y => y.Action == FileActions.Added)),
                            Times.Once);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(null, It.Is<TrackedFileAction>(y => y.FileLastModifiedDateUtc == writeTime)),
                            Times.Once);
        }

        [TestMethod]
        public void Action_Added_when_Tracked_File_Isnt_Found()
        {
            DateTime writeTime = DateTime.Now.ToUniversalTime();
            var filePath = Path.Combine(BaseDirectory, "abc.def");
            var content = new byte[] { 1, 1, 1 };
            var instance = _mocker.Create<DirectoryCrawler>();

            var trackedFile = new TrackedFile
            {
                Actions = new List<TrackedFileAction>(new[] 
                {
                    new TrackedFileAction 
                    {
                        Path = BaseDirectory,
                        FileName = Path.GetFileName(filePath),
                        Action = FileActions.Added,
                        FileLength = content.Length,
                        FileLastModifiedDateUtc = writeTime,
                        EffectiveDateUtc = DateTime.Now.ToUniversalTime()
                    }
                })
            };

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(BaseDirectory))
                   .Returns(new[] { trackedFile });

            instance.CheckDirectoryContents(BaseDirectory);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Deleted)),
                            Times.Once);
        }
    }
}
