using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Backplan.Client.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMoq;
using Moq;

namespace Backplan.Client.Tests.IO
{
    [TestClass]
    public class DirectoryWatcherTests
    {
        private AutoMoqer _mocker;
        private DirectoryWatcher _instance;

        [TestInitialize]
        public void Setup()
        {
            _mocker = new AutoMoqer();
            
            // GetMock of the abstract class before create to prevent automoq bugs
            _mocker.GetMock<FileSystemWatcherBase>();

            _instance = _mocker.Create<DirectoryWatcher>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Throws_Exception_If_Begin_Called_With_Null_Path()
        {
            _instance.Begin(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Throws_Exception_If_Begin_Called_With_Empty_Path()
        {
            _instance.Begin("");
        }

        [TestMethod]
        public void Watcher_Gets_Path_Set_Same_As_Begin_Path_Parameter()
        {
            const string path = @"C:\test";
            _mocker.GetMock<FileSystemWatcherBase>().SetupAllProperties();
            _instance.Begin(path);

            _mocker.GetMock<FileSystemWatcherBase>()
                   .VerifySet(x => x.Path = path);
        }
    }
}
