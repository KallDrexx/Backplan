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
using Backplan.Client.Models;
using SystemWrapper;
using AutoMoq;
using Moq;

namespace Backplan.Client.Tests.IO
{
    [TestClass]
    public class DirectoryCrawlerTests
    {
        private AutoMoqer _mocker;

        [TestInitialize]
        public void Setup()
        {
            _mocker = new AutoMoqer();
        }

        [TestMethod]
        public void Requests_Tracked_Files_In_Path()
        {
            var instance = _mocker.Create<DirectoryCrawler>();
            instance.CheckDirectoryContents("C:\\Test");

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.GetTrackedFilesInPath(@"C:\Test"), Times.Once);
        }

        [TestMethod]
        public void Requests_Tracked_Files_In_Subfolder()
        {
            var instance = _mocker.Create<DirectoryCrawler>();
            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFilesInPath(@"C:\Test"))
                   .Returns(new string[] { @"C:\Test\Directory" });

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.IsDirectory(@"C:\Test\Directory"))
                   .Returns(true);

            instance.CheckDirectoryContents("C:\\Test");

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.GetTrackedFilesInPath(@"C:\Test\Directory"), Times.Once);
        }

        [TestMethod]
        public void No_Action_Added_When_Existing_File_Not_Changed()
        {
            var instance = _mocker.Create<DirectoryCrawler>();

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

            var fileInfoMock = new Mock<IFileInfoWrap>();
            fileInfoMock.Setup(x => x.Length).Returns(fileLength);
            fileInfoMock.Setup(x => x.LastWriteTimeUtc).Returns(new DateTimeWrap(writeTime));
            fileInfoMock.Setup(x => x.Name).Returns(fileName);
            fileInfoMock.Setup(x => x.DirectoryName).Returns(filePath);
            fileInfoMock.Setup(x => x.Exists).Returns(true);

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(filePath))
                   .Returns(new[] { trackedFile });

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                   .Returns(fileInfoMock.Object);
            
            instance.CheckDirectoryContents(filePath);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(It.IsAny<TrackedFile>(), It.IsAny<TrackedFileAction>()), Times.Never);
        }

        [TestMethod]
        public void Action_Added_When_File_Size_Doesnt_Match()
        {
            var instance = _mocker.Create<DirectoryCrawler>();

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

            var fileInfoMock = new Mock<IFileInfoWrap>();
            fileInfoMock.Setup(x => x.Length).Returns(fileLength + 1);
            fileInfoMock.Setup(x => x.LastWriteTimeUtc).Returns(new DateTimeWrap(writeTime));
            fileInfoMock.Setup(x => x.Name).Returns(fileName);
            fileInfoMock.Setup(x => x.DirectoryName).Returns(filePath);
            fileInfoMock.Setup(x => x.Exists).Returns(true);

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(filePath))
                   .Returns(new[] { trackedFile });

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                   .Returns(fileInfoMock.Object);

            instance.CheckDirectoryContents(filePath);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Modified)),
                            Times.Once);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.FileLength == fileLength + 1)),
                            Times.Once);
        }

        [TestMethod]
        public void Action_Added_When_File_Modified_Date_Is_Newer()
        {
            var instance = _mocker.Create<DirectoryCrawler>();

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

            var fileInfoMock = new Mock<IFileInfoWrap>();
            fileInfoMock.Setup(x => x.Length).Returns(fileLength);
            fileInfoMock.Setup(x => x.LastWriteTimeUtc).Returns(new DateTimeWrap(writeTime.AddDays(1)));
            fileInfoMock.Setup(x => x.Name).Returns(fileName);
            fileInfoMock.Setup(x => x.DirectoryName).Returns(filePath);
            fileInfoMock.Setup(x => x.Exists).Returns(true);

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(filePath))
                   .Returns(new[] { trackedFile });

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                   .Returns(fileInfoMock.Object);

            instance.CheckDirectoryContents(filePath);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Modified)),
                            Times.Once);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.FileLastModifiedDateUtc == writeTime.AddDays(1))),
                            Times.Once);
        }

        [TestMethod]
        public void Action_Added_When_File_Isnt_In_Tracked_List()
        {
            var instance = _mocker.Create<DirectoryCrawler>();

            const string filePath = @"C:\Test";
            const string fileName = "abc.def";
            const int fileLength = 100;
            DateTime writeTime = DateTime.Now.ToUniversalTime();

            var fileInfoMock = new Mock<IFileInfoWrap>();
            fileInfoMock.Setup(x => x.Length).Returns(fileLength);
            fileInfoMock.Setup(x => x.LastWriteTimeUtc).Returns(new DateTimeWrap(writeTime.AddDays(1)));
            fileInfoMock.Setup(x => x.Name).Returns(fileName);
            fileInfoMock.Setup(x => x.DirectoryName).Returns(filePath);
            fileInfoMock.Setup(x => x.Exists).Returns(true);

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(filePath))
                   .Returns(new TrackedFile[] {});

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                   .Returns(fileInfoMock.Object);

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFilesInPath(filePath))
                   .Returns(new string[] { fileName });

            instance.CheckDirectoryContents(filePath);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(null, It.Is<TrackedFileAction>(y => y.Action == FileActions.Added)),
                            Times.Once);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(null, It.Is<TrackedFileAction>(y => y.FileLastModifiedDateUtc == writeTime.AddDays(1))),
                            Times.Once);
        }

        [TestMethod]
        public void Action_Added_when_Tracked_File_Isnt_Found()
        {
            var instance = _mocker.Create<DirectoryCrawler>();

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

            var fileInfoMock = new Mock<IFileInfoWrap>();
            fileInfoMock.Setup(x => x.Exists).Returns(false);

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFilesInPath(filePath))
                   .Returns(new[] { trackedFile });

            _mocker.GetMock<IPathDetails>()
                   .Setup(x => x.GetFileInfo(Path.Combine(filePath, fileName)))
                   .Returns(fileInfoMock.Object);

            instance.CheckDirectoryContents(filePath);

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Deleted)),
                            Times.Once);
        }
    }
}
