using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spi.Data;

namespace TestListDiff
{
    [TestClass]
    public class Test_DeluxeIterator
    {
        [TestMethod]
        public void Test_Creation()
        {
            var iterDeluxe = new DeluxeEnumerator<char>( "berni" );
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Test_Begin_1()
        {
            var iterDeluxe = new DeluxeEnumerator<char>("berni");
            iterDeluxe.MoveNext();
            Assert.AreEqual('b', iterDeluxe.Current);
            Assert.AreEqual(default(char), iterDeluxe.LastValue);
        }
        [TestMethod]
        public void Test_Last_1()
        {
            var iterDeluxe = new DeluxeEnumerator<char>("berni");
            iterDeluxe.MoveNext();
            Assert.AreEqual('b', iterDeluxe.Current);
            iterDeluxe.MoveNext();
            Assert.AreEqual('b', iterDeluxe.LastValue);
        }
        [TestMethod]
        public void Test_Empty()
        {
            var iterDeluxe = new DeluxeEnumerator<char>("");
            bool hasMore = iterDeluxe.MoveNext();
            Assert.AreEqual(false, hasMore);
        }
        [TestMethod]
        public void Test_hasMore_1()
        {
            var iterDeluxe = new DeluxeEnumerator<char>("");
            iterDeluxe.MoveNext();
            Assert.AreEqual(false, iterDeluxe.HasMoved);
        }
        public void Test_hasMore_2()
        {
            var iterDeluxe = new DeluxeEnumerator<char>("x");
            iterDeluxe.MoveNext();
            Assert.AreEqual(true, iterDeluxe.HasMoved);
            iterDeluxe.MoveNext();
            Assert.AreEqual(false, iterDeluxe.HasMoved);
        }
        [TestMethod]
        public void Test_Seq_1()
        {
            var iterDeluxe = new DeluxeEnumerator<char>("berni");
            iterDeluxe.MoveNext();
            Assert.AreEqual('b', iterDeluxe.Current);
            iterDeluxe.MoveNext();
            Assert.AreEqual('b', iterDeluxe.LastValue);
            Assert.AreEqual('e', iterDeluxe.Current);
            iterDeluxe.MoveNext();
            Assert.AreEqual('e', iterDeluxe.LastValue);
            Assert.AreEqual('r', iterDeluxe.Current);
            iterDeluxe.MoveNext();
            Assert.AreEqual('r', iterDeluxe.LastValue);
            Assert.AreEqual('n', iterDeluxe.Current);
            iterDeluxe.MoveNext();
            Assert.AreEqual('n', iterDeluxe.LastValue);
            Assert.AreEqual('i', iterDeluxe.Current);
            bool hasMore = iterDeluxe.MoveNext();
            Assert.AreEqual('i', iterDeluxe.LastValue);
            Assert.AreEqual(false, hasMore);
        }
    }
}
