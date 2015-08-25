using Spi.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SpiTest
{
    
    
    /// <summary>
    ///This is a test class for MiscTest and is intended
    ///to contain all MiscTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MiscTest
    {
        private enum PRE_DIR_ACTION
        {
            DELETE,
            CREATE
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        /// <summary>
        ///A test for GetLongFilenameNotation
        ///</summary>
        [TestMethod()]
        public void GetLongFilenameNotationTest()
        {
            Assert.AreEqual(@"",                Misc.GetLongFilenameNotation(@""));
            Assert.AreEqual(@".\dir",           Misc.GetLongFilenameNotation(@".\dir"));
            Assert.AreEqual(@"\\?\c:\",         Misc.GetLongFilenameNotation(@"c:\"));
            Assert.AreEqual(@"\\?\c:\bumsti",   Misc.GetLongFilenameNotation(@"c:\bumsti"));
            Assert.AreEqual(@"\\?\c:\",         Misc.GetLongFilenameNotation(@"\\?\c:\"));

            Assert.AreEqual(@"\\?\UNC\server\share", Misc.GetLongFilenameNotation(@"\\server\share"));
            Assert.AreEqual(@"\\?\UNC\server\share", Misc.GetLongFilenameNotation(@"\\?\UNC\server\share"));

        }
        [TestMethod]
        public void CreatePath()
        {
            Assert.IsFalse(Spi.IO.Misc.CreatePath(@"g:"));      // "G:" is non-existing!!!!! On my maschine!!!!
            Assert.IsFalse(Spi.IO.Misc.CreatePath(@"\\?\c:"));  // return false because it's not a dir
            Assert.IsTrue(Spi.IO.Misc.CreatePath(@"f:"));       // "{Driveletter}:"  ... is a dir --> ok
            Assert.IsTrue(Spi.IO.Misc.CreatePath(@"f:\"));      // "{Driveletter}:\" ... is a dir --> ok
            Assert.IsFalse(Spi.IO.Misc.CreatePath(@"cc:"));     // "invalid parameter"
            Assert.IsTrue(Spi.IO.Misc.CreatePath(@"\"));        // is a dir
            Assert.IsFalse(Spi.IO.Misc.CreatePath(@"\\?\"));
            Assert.IsFalse(Spi.IO.Misc.CreatePath(@"\\?\\"));
            Assert.IsFalse(Spi.IO.Misc.CreatePath(@"\\?\C"));

            internal_CreatePath(@"f:\jucksi\xxx","", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"f:\jucksi", "", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"f:\flupsi", "", PRE_DIR_ACTION.DELETE);


            internal_CreatePath(@"c:\temp\a","", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"c:\temp\b", "", PRE_DIR_ACTION.CREATE);
            internal_CreatePath(@"c:\temp\c", "b", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"c:\temp\d", @"b\aaa\ggg\eee", PRE_DIR_ACTION.DELETE);

            internal_CreatePath(@"\\?\c:\temp\e", "", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"\\?\c:\temp\f", @"spindi\bumsti\1", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"\\?\c:\temp\g", @"spindi\bumsti\1", PRE_DIR_ACTION.CREATE);

            internal_CreatePath(@"\\localhost\c$\spindi\h", "", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"\\localhost\c$\spindi\i", @"jucksi\flucksi", PRE_DIR_ACTION.DELETE);

            internal_CreatePath(@"\\?\UNC\localhost\c$\spindi\j", "", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"\\?\UNC\localhost\c$\spindi\k", @"ramsi\wamsi\gamsi", PRE_DIR_ACTION.DELETE);
            internal_CreatePath(@"\\?\UNC\localhost\c$\spindi", "", PRE_DIR_ACTION.CREATE);
            internal_CreatePath(@"\\?\UNC\localhost\c$\spindi", "", PRE_DIR_ACTION.DELETE);
            
        }
        private void internal_CreatePath(string baseDir, string dirsToTest, PRE_DIR_ACTION action)
        {
            if ( baseDir.EndsWith(@":\") ) 
            {
                Assert.Fail("this would delete [{0}]. not good", baseDir);
                return;
            }

            string CompleteDir = baseDir + 
                ( String.IsNullOrEmpty(dirsToTest) ? "" : System.IO.Path.DirectorySeparatorChar + dirsToTest );
            string Shortname = RemoveLongnameNotation(CompleteDir);
            
            switch (action)
            {
                case PRE_DIR_ACTION.CREATE:
                    
                    System.IO.Directory.CreateDirectory(Shortname);
                    break;
                case PRE_DIR_ACTION.DELETE:
                    DelDir(baseDir);
                    break;
                default:
                    throw new Exception("illegal enum value for PRE_DIR_ACTION");
            }
            //
            // THE TEST!!!
            //
            Assert.IsTrue( Spi.IO.Misc.CreatePath(CompleteDir) );
            Assert.IsTrue( System.IO.Directory.Exists(Shortname) );

            DelDir(baseDir);
        }
        private void DelDir(string Dir)
        {
            try
            {
                string Shortname = RemoveLongnameNotation(Dir);
                if (System.IO.Directory.Exists(Shortname))
                {
                    System.IO.Directory.Delete(Shortname, true);
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                // wurscht
            }
        }
        private string RemoveLongnameNotation(string Dir)
        {
            if (Dir.StartsWith(@"\\?\UNC\"))
            {
                return @"\\" + Dir.Substring(8);
                
            }
            else if (Dir.StartsWith(@"\\?\"))
            {
                return Dir.Substring(4);
            }
            return Dir;
        }
    }
}
