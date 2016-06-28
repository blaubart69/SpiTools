using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spi.Data;

namespace TestListDiff
{
    [TestClass]
    public class TestDiffer
    {
        uint NumberDiffs;

        [TestInitialize]
        public void Setup()
        {
        }
        [TestMethod]
        public void Test_Delta_OnlyTwoElements()
        {
            IList<BOCmp> a = new List<BOCmp>()
            {
                new BOCmp() { Name="adam", Edition=1},
                new BOCmp() { Name="bumsti", Edition=1}
            };
            IList<BOCmp> b = new List<BOCmp>()
            {
                new BOCmp() { Name="bumsti", Edition=2}
            };

            var result = DoDelta(a, b, out NumberDiffs);
            var expected = new List<Tuple<DELTA_STATE,BOCmp>>()
            {
                 new Tuple<DELTA_STATE, BOCmp>(DELTA_STATE.DELETE, new BOCmp() { Name="adam", Edition=1 })
                ,new Tuple<DELTA_STATE, BOCmp>(DELTA_STATE.MODIFY, new BOCmp() { Name="bumsti", Edition=2 })
            };

            Assert.AreEqual<uint>(2, NumberDiffs);
            Assert.IsTrue(MyCollAssert(expected, result));
        }
        [TestMethod]
        public void Test_Delta_EmptyLists()
        {
            var result = DoDelta(new List<BOCmp>(), new List<BOCmp>(), out NumberDiffs);
            Assert.AreEqual<uint>(0,NumberDiffs);
            Assert.IsTrue(result.Count == 0);
        }
        [TestMethod]
        public void Test_Delta_OneElementIsDeleted()
        {
            var result = DoDelta(
            new List<BOCmp>() {
                new BOCmp() { Name="Hugo", Edition=1 }
            }, 
            new List<BOCmp>(),
            out NumberDiffs);

            var expected = new List<Tuple<DELTA_STATE, BOCmp>>()
            {
                CrtTup(DELTA_STATE.DELETE, "Hugo", 1)
            };

            Assert.AreEqual<uint>(1, NumberDiffs);
            Assert.IsTrue( MyCollAssert(expected, result) );
        }
        [TestMethod]
        public void Test_Delta_OneElementIsAdded()
        {
            var result = DoDelta(
                new List<BOCmp>(),
                new List<BOCmp>() 
                { 
                    new BOCmp() { Name="Hugo", Edition=1 }
                },
                out NumberDiffs
            );

            var expected = new List<Tuple<DELTA_STATE, BOCmp>>()
            {
                CrtTup(DELTA_STATE.NEW, "Hugo", 1)
            };
            Assert.AreEqual<uint>(1, NumberDiffs);
            Assert.IsTrue(MyCollAssert(expected, result));
        }
        [TestMethod]
        public void Test_Delta_OneElementIsModified()
        {
            var result = DoDelta(
                new List<BOCmp>()
                {
                    new BOCmp() { Name="Hugo", Edition=1 }
                },
                new List<BOCmp>() 
                { 
                    new BOCmp() { Name="Hugo", Edition=2 }
                },
                out NumberDiffs
            );

            var expected = new List<Tuple<DELTA_STATE, BOCmp>>()
            {
                CrtTup(DELTA_STATE.MODIFY, "Hugo", 2)
            };
            Assert.AreEqual<uint>(1, NumberDiffs);
            Assert.IsTrue(MyCollAssert(expected, result));
        }
        [TestMethod]
        public void Test_Delta_ListAreEqualWithOneElementInEachList()
        {
            var result = DoDelta(
                new List<BOCmp>() { new BOCmp() { Name="Hugo", Edition=1 }  },
                new List<BOCmp>() { new BOCmp() { Name="Hugo", Edition=1 }  },
                out NumberDiffs
            );

            var expected = new List<Tuple<DELTA_STATE, BOCmp>>() { CrtTup(DELTA_STATE.SAMESAME, "Hugo", 1) };
            Assert.AreEqual<uint>(0, NumberDiffs);
            Assert.IsTrue(MyCollAssert(expected, result));
        }
        [TestMethod]
        public void Test_Delta_ListAreEqualWithTwoElementInEachList()
        {
            var result = DoDelta(
                new List<BOCmp>()
                {
                     new BOCmp() { Name="Hugo", Edition=1 }
                    ,new BOCmp() { Name="", Edition=2 }
                },
                new List<BOCmp>() 
                { 
                     new BOCmp() { Name="Hugo", Edition=1 }
                    ,new BOCmp() { Name="", Edition=2 }
                },
                out NumberDiffs
            );

            var expected = new List<Tuple<DELTA_STATE, BOCmp>>()
                {
                    CrtTup(DELTA_STATE.SAMESAME, "Hugo", 1),
                    CrtTup(DELTA_STATE.SAMESAME, "", 2)
                };
            Assert.AreEqual<uint>(0, NumberDiffs);
            Assert.IsTrue(MyCollAssert(expected, result));
        }
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private List<Tuple<DELTA_STATE, BOCmp>> DoDelta(IList<BOCmp> a, IList<BOCmp> b, out uint differences)
        {
            var result = new List<Tuple<DELTA_STATE, BOCmp>>();

            differences = Spi.Data.Delta.WalkSortedLists<BOCmp, BOCmp, object>(a, b,
                (BOCmp obja, BOCmp objb) =>
                {
                    int cmp = obja.Name.CompareTo(objb.Name);
                    if (cmp != 0)
                    {
                        return cmp < 0 ? DELTA_COMPARE_RESULT.LESS : DELTA_COMPARE_RESULT.GREATER;
                    }
                    else
                    {
                        int cmpEdt = obja.Edition.CompareTo(objb.Edition);
                        return cmpEdt == 0 ? DELTA_COMPARE_RESULT.EQUAL : DELTA_COMPARE_RESULT.MODIFY;
                    }
                },
               (DELTA_STATE state, BOCmp obja, BOCmp objb, object context) =>
               {
                   BOCmp ToAdd = null;
                   switch (state)
                   {
                       case DELTA_STATE.MODIFY:
                       case DELTA_STATE.NEW: ToAdd = objb; break;
                       case DELTA_STATE.DELETE: ToAdd = obja; break;
                       case DELTA_STATE.SAMESAME: ToAdd = obja; break;
                   }
                   result.Add(new Tuple<DELTA_STATE, BOCmp>(state, ToAdd));
               },
               new object());
            return result;
        }
        private bool MyCollAssert(IList<Tuple<DELTA_STATE, BOCmp>> expected, IList<Tuple<DELTA_STATE, BOCmp>> result)
        {
            if (expected.Count != result.Count)
            {
                Assert.Fail("expected count [{0}] vs result count [{1}]", expected.Count, result.Count);
                return false;
            }
            for (int i = 0; i < expected.Count; i++)
            {
                if (expected[i].Item1.CompareTo(result[i].Item1) != 0)
                {
                    Assert.Fail("DeltaState! Idx [{0}] expected [{1}|{2}|{3}] result [{4}|{5}|{6}]", 
                        i,
                        expected[i].Item1,  expected[i].Item2.Name,  expected[i].Item2.Edition,
                        result[i].Item1,    result[i].Item2.Name,    result[i].Item2.Edition);
                    return false;
                }
                if (expected[i].Item2.CompareTo(result[i].Item2) != 0)
                {
                    Assert.Fail("Name/Edt! Idx [{0}] expected [{1}|{2}|{3}] result [{4}|{5}|{6}]",
                        i,
                        expected[i].Item1, expected[i].Item2.Name, expected[i].Item2.Edition,
                        result[i].Item1, result[i].Item2.Name, result[i].Item2.Edition);
                    return false;
                }
            }
            return true;
        }
        private Tuple<DELTA_STATE, BOCmp> CrtTup(DELTA_STATE ds, string name, int edt)
        {
            return new Tuple<DELTA_STATE, BOCmp>(ds, new BOCmp() { Name = name, Edition = edt });
        }
    }
}
