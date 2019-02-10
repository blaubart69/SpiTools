using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Spi;

namespace DeltaTest
{
    [TestClass]
    public class TestDelta
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException),"A keyComparison of null was inappropriately allowed.")]
        public void KeyComparisonNullException()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            DiffAscendingSortedLists.Run<int,string>(
                ListA: numbers, 
                ListB: strings, 
                KeyComparison: null, 
                KeySelfComparerA: null, 
                KeySelfComparerB: null, 
                AttributeComparer: null,
                checkSortOrder: false, 
                OnCompared: null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "A OnCompared callback of null was inappropriately allowed.")]
        public void OnComparedNullException()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            DiffAscendingSortedLists.Run<int, string>(
                ListA: numbers,
                ListB: strings,
                KeyComparison: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySelfComparerA: null,
                KeySelfComparerB: null,
                AttributeComparer: null,
                checkSortOrder: false,
                OnCompared: null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "A KeySelfComparer callback of null was inappropriately allowed.")]
        public void SelfComparerNullExceptionButCheckSortOrderTrue()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            DiffAscendingSortedLists.Run<int, string>(
                ListA: numbers,
                ListB: strings,
                KeyComparison: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySelfComparerA: null,
                KeySelfComparerB: null,
                AttributeComparer: null,
                checkSortOrder: true,
                OnCompared: (state, num, str) => { });
        }
        [TestMethod]
        public void NoExceptionWhenSelfComparerIsNullButSortOrderIsFalse()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            DiffAscendingSortedLists.Run<int, string>(
                ListA: numbers,
                ListB: strings,
                KeyComparison: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySelfComparerA: null,
                KeySelfComparerB: null,
                AttributeComparer: null,
                checkSortOrder: false,
                OnCompared: (state, num, str) => { });
        }

        [TestMethod]
        public void TwoDifferentTypesButAllTheSame()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            uint differences =
            DiffAscendingSortedLists.Run<int,string>(numbers, strings,
                KeyComparison: (numberA, stringB) => numberA.CompareTo( int.Parse(stringB) ),
                KeySelfComparerA: (int    i, int    k)  => i.CompareTo(k),
                KeySelfComparerB: (string i, string k)  => String.Compare(i, k),
                AttributeComparer: null,
                checkSortOrder: true,
                OnCompared: (Spi.DIFF_STATE state, int num, string str) =>
                {
                });

            Assert.AreEqual<uint>(0, differences);

        }
        [TestMethod]
        public void TwoDifferentTypesWithOneDifference()
        {
            int[]    numbers = new int[]    {  1,   2       };
            string[] strings = new string[] { "1", "2", "3" };

            List<string> newItemsInB = new List<string>();

            uint differences =
            DiffAscendingSortedLists.Run<int,string>(
                numbers, strings,
                KeyComparison: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                AttributeComparer: null,
                KeySelfComparerA: (int    i, int    k)  => i.CompareTo(k),
                KeySelfComparerB: (string i, string k)  => String.Compare(i, k),
                checkSortOrder: true,
                OnCompared: (DIFF_STATE state, int num, string str) =>
                {
                    if ( state == DIFF_STATE.NEW_B)
                    {
                        newItemsInB.Add(str);
                    }
                });

            Assert.AreEqual<uint>(1, differences);
            Assert.AreEqual(1, newItemsInB.Count);
            Assert.AreEqual("3", newItemsInB[0]);
        }
        [TestMethod]
        public void TwoDifferentTypesWithTwoDifference()
        {
            int[]    Anumbers = new int[]    {  1,   2       };
            string[] Bstrings = new string[] {      "2", "3" };

            List<string> newItemsInB = new List<string>();
            List<int> delItemsInA = new List<int>();

            uint differences =
            DiffAscendingSortedLists.Run<int,string>(
                Anumbers, Bstrings,
                KeyComparison: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                AttributeComparer: null,
                KeySelfComparerA: (int i, int k) => i.CompareTo(k),
                KeySelfComparerB: (string i, string k) => String.Compare(i, k),
                checkSortOrder: true,
                OnCompared: (DIFF_STATE state, int num, string str) =>
                {
                    if (state == DIFF_STATE.NEW_B)
                    {
                        newItemsInB.Add(str);
                    }
                    else if (state == DIFF_STATE.DELETE_A)
                    {
                        delItemsInA.Add(num);
                    }
                });

            Assert.AreEqual<uint>(2, differences);
            Assert.AreEqual(1, newItemsInB.Count);
            Assert.AreEqual("3", newItemsInB[0]);
            Assert.AreEqual(1, delItemsInA.Count);
            Assert.AreEqual(1, delItemsInA[0]);
        }
        [TestMethod]
        public void TwoDifferentTypesWithModifications()
        {
            double[] Anumbers = new double[] {  1,   2.5f };
            string[] Bstrings = new string[] { "1", "2,3" };

            List<string> newItemsInB = new List<string>();
            List<double> delItemsInA = new List<double>();
            List<Tuple<double, string>> modList = new List<Tuple<double, string>>();

            uint differences =
            DiffAscendingSortedLists.Run(
                Anumbers, Bstrings,
                KeyComparison: (numberA, stringB) =>
                {
                    int numberIntA = (int)(numberA);
                    int numberIntB = (int)(double.Parse(stringB));
                    return numberIntA.CompareTo(numberIntB);
                },
                AttributeComparer: (double a, string b) =>
                {
                    double restA = a - Math.Floor(a);

                    string[] parts = b.Split(',');
                    string restB = parts.Length == 2 ? parts[1] : String.Empty;
                    double dblRestB = Convert.ToDouble("0," + restB);

                    return restA.Equals(dblRestB);
                },
                KeySelfComparerA: (double i, double k) => i.CompareTo(k),
                KeySelfComparerB: (string i, string k) => String.Compare(i, k),
                checkSortOrder: true,
                OnCompared: (DIFF_STATE state, double num, string str) =>
                {
                    if (state == DIFF_STATE.MODIFY)
                    {
                        modList.Add(Tuple.Create(num,str));
                    }
                });

            Assert.AreEqual<uint>(1, differences);
            Assert.AreEqual(0, newItemsInB.Count);
            Assert.AreEqual(0, delItemsInA.Count);
            Assert.AreEqual(1, modList.Count);
            Assert.AreEqual(2.5f, modList[0].Item1);
            Assert.AreEqual("2,3", modList[0].Item2);
        }
    }
}
