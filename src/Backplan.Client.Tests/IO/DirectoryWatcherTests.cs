using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Backplan.Client.IO;
using Backplan.Client.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMoq;
using Moq;
using Backplan.Client.Database;

namespace Backplan.Client.Tests.IO
{
    [TestClass]
    public class DirectoryWatcherTests
    {
        private const string Path = @"C:\test";
        private const string FileName = "abc.def";

        private AutoMoqer _mocker;
        private DirectoryWatcher _instance;
        private int _expectedFileLength;
        private DateTime _expectedWriteDate;
        private TrackedFile _trackedFile;

        [TestInitialize]
        public void Setup()
        {
            _mocker = new AutoMoqer();

            var mockFileSystem = new MockFileSystem();
            _mocker.SetInstance<IFileSystem>(mockFileSystem);
            
            // GetMock of the abstract class before create to prevent automoq bugs
            _mocker.GetMock<FileSystemWatcherBase>();

            _instance = _mocker.Create<DirectoryWatcher>();

            // Mocked files
            var content = new byte[] {1, 1, 1};
            _expectedFileLength = content.Length;
            _expectedWriteDate = DateTime.Now.ToUniversalTime();

            var nameWithPath = mockFileSystem.Path.Combine(Path, FileName);
            mockFileSystem.AddFile(nameWithPath, new MockFileData(content)
            {
                LastWriteTime = _expectedWriteDate
            });

            _trackedFile = new TrackedFile();
            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFileByFullPath(nameWithPath))
                   .Returns(_trackedFile);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Throws_Exception_If_Start_Called_With_Null_Path()
        {
            _instance.Start(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Throws_Exception_If_Start_Called_With_Empty_Path()
        {
            _instance.Start("");
        }

        [TestMethod]
        public void Watcher_Gets_Path_Set_Same_As_Start_Path_Parameter()
        {
            _instance.Start(Path);

            _mocker.GetMock<FileSystemWatcherBase>()
                   .VerifySet(x => x.Path = Path);
        }

        [TestMethod]
        public void Watcher_Is_Enabled_From_Start_Method()
        {
            _instance.Start(Path);

            _mocker.GetMock<FileSystemWatcherBase>()
                   .VerifySet(x => x.EnableRaisingEvents = true);
        }

        [TestMethod]
        public void Watcher_Disabled_When_Disposed()
        {
            _instance.Dispose();

            _mocker.GetMock<FileSystemWatcherBase>()
                   .VerifySet(x => x.EnableRaisingEvents = false);
        }

        [TestMethod]
        public void Tracked_File_Action_Added_When_File_Created()
        {
            _instance.Start(Path);
            _mocker.GetMock<FileSystemWatcherBase>()
                   .Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, Path, FileName));

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(null, It.Is<TrackedFileAction>(y => y.Action == FileActions.Added &&
                                                                                                 y.FileName == FileName &&
                                                                                                 y.Path == Path &&
                                                                                                 y.FileLength == _expectedFileLength &&
                                                                                                 y.FileLastModifiedDateUtc == _expectedWriteDate)));
        }

        [TestMethod]
        public void Tracked_File_Action_Added_When_File_Changed()
        {
            _instance.Start(Path);
            _mocker.GetMock<FileSystemWatcherBase>()
                   .Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path, FileName));

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(_trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Modified &&
                                                                                                 y.FileName == FileName &&
                                                                                                 y.Path == Path &&
                                                                                                 y.FileLength == _expectedFileLength &&
                                                                                                 y.FileLastModifiedDateUtc == _expectedWriteDate)));
        }

        [TestMethod]
        public void Tracked_File_Action_Added_When_File_Deleted()
        {
            _instance.Start(Path);
            _mocker.GetMock<FileSystemWatcherBase>()
                   .Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path, FileName));

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(_trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Deleted &&
                                                                                                 y.FileName == FileName &&
                                                                                                 y.Path == Path &&
                                                                                                 y.FileLength == _expectedFileLength &&
                                                                                                 y.FileLastModifiedDateUtc == _expectedWriteDate)));
        }

        [TestMethod]
        public void Tracked_File_Action_Added_When_File_Renamed()
        {
            const string oldName = "eee.def";
            const string oldNameWithPath = @"C:\test\eee.def";

            _mocker.GetMock<ITrackedFileStore>()
                   .Setup(x => x.GetTrackedFileByFullPath(oldNameWithPath))
                   .Returns(_trackedFile);

            _instance.Start(Path);
            _mocker.GetMock<FileSystemWatcherBase>()
                   .Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, Path, FileName, oldName));

            _mocker.GetMock<ITrackedFileStore>()
                   .Verify(x => x.AddFileActionToTrackedFile(_trackedFile, It.Is<TrackedFileAction>(y => y.Action == FileActions.Renamed &&
                                                                                                 y.FileName == FileName &&
                                                                                                 y.Path == Path &&
                                                                                                 y.FileLength == _expectedFileLength &&
                                                                                                 y.FileLastModifiedDateUtc == _expectedWriteDate)));
        }
    }
}