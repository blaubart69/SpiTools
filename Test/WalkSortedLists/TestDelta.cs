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

            DiffAscendingSortedLists.Run<int,string,object>(
                ListA: numbers, 
                ListB: strings, 
                KeyComparer: null,
                KeySortOrderComparerA: null, 
                KeySortOrderComparerB: null, 
                AttributeComparer: null,
                OnNewB: null, OnDeleteA: null, OnModified: null, OnSameSame: null);
        }
        /*
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "A OnCompared callback of null was inappropriately allowed.")]
        public void OnComparedNullException()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            DiffAscendingSortedLists.NoModified<int, string>(
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
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySortOrderComparerA: null,
                KeySortOrderComparerB: null,
                AttributeComparer: null,
                checkSortOrder: true,
                OnCompared: (state, num, str) => { });
        }*/
        [TestMethod]
        public void NoExceptionWhenSelfComparerIsNull()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            DiffAscendingSortedLists.NoModified<int, string>(
                ListA: numbers,
                ListB: strings,
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySortOrderComparerA: null,
                KeySortOrderComparerB: null,
                OnNewB: null, OnDeleteA: null, OnSameSame: null);
        }

        [TestMethod]
        public void TwoDifferentTypesButAllTheSame()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            uint differences = 0;
            DiffAscendingSortedLists.NoModified<int, string>(numbers, strings,
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySortOrderComparerA: (int i, int k) => i.CompareTo(k),
                KeySortOrderComparerB: (string i, string k) => String.Compare(i, k),
                OnNewB: b => differences++,
                OnDeleteA: a => differences++,
                OnSameSame: null);

            Assert.AreEqual<uint>(0, differences);
        }
        [TestMethod]
        public void TwoDifferentTypesWithOneDifference()
        {
            int[]    numbers = new int[]    {  1,   2       };
            string[] strings = new string[] { "1", "2", "3" };

            List<string> newItemsInB = new List<string>();

            uint differences = 0;
            DiffAscendingSortedLists.NoModified<int, string>(
                numbers, strings,
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySortOrderComparerA: (int i, int k) => i.CompareTo(k),
                KeySortOrderComparerB: (string i, string k) => String.Compare(i, k),
                OnNewB: b => { newItemsInB.Add(b); differences++; },
                OnDeleteA: null, OnSameSame: null);

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

            uint differences = 0;
            DiffAscendingSortedLists.NoModified<int, string>(
                Anumbers, Bstrings,
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                KeySortOrderComparerA: (int i, int k) => i.CompareTo(k),
                KeySortOrderComparerB: (string i, string k) => String.Compare(i, k),
                OnNewB: bstring => { newItemsInB.Add(bstring); differences++; },
                OnDeleteA: aint => { delItemsInA.Add(aint); differences++; },
                OnSameSame: null);

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

            uint differences = 0;
            DiffAscendingSortedLists.Run(
                Anumbers, Bstrings,
                KeyComparer: (numberA, stringB) =>
                {
                    int numberIntA = (int)(numberA);
                    int numberIntB = (int)(double.Parse(stringB));
                    return numberIntA.CompareTo(numberIntB);
                },
                AttributeComparer: (double a, string b, out double diff) =>
                {
                    double restA = a - Math.Floor(a);

                    string[] parts = b.Split(',');
                    string restB = parts.Length == 2 ? parts[1] : String.Empty;
                    double dblRestB = Convert.ToDouble("0," + restB);

                    diff = dblRestB - restA;

                    return restA.Equals(dblRestB);
                },
                KeySortOrderComparerA: (double i, double k) => i.CompareTo(k),
                KeySortOrderComparerB: (string i, string k) => String.Compare(i, k),
                OnModified: (double num, string str, double diff) => { modList.Add(Tuple.Create(num, str)); differences++; },
                OnNewB: null,
                OnDeleteA: null,
                OnSameSame: null);

            Assert.AreEqual<uint>(1, differences);
            Assert.AreEqual(0, newItemsInB.Count);
            Assert.AreEqual(0, delItemsInA.Count);
            Assert.AreEqual(1, modList.Count);
            Assert.AreEqual(2.5f, modList[0].Item1);
            Assert.AreEqual("2,3", modList[0].Item2);
        }
    }
}
