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
        public void TwoDifferentTypesButAllTheSame()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            string[] strings = new string[] { "1", "2", "3" };

            uint differences =
            Diff.DiffSortedEnumerables<int,string, int, string, object, object>(numbers, strings,
                KeySelectorA: itemA => itemA,
                KeySelectorB: itemB => itemB,
                KeyComparer: (numberA, stringB) => numberA.CompareTo( int.Parse(stringB) ),
                AttributeSelectorA: null,
                AttributeSelectorB: null,
                AttributeComparer: null,
                KeySelfComparerA: (int    i, int    k)  => i.CompareTo(k),
                KeySelfComparerB: (string i, string k)  => String.Compare(i, k),
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
            Diff.DiffSortedEnumerables<int, string, int, string, object, object>(
                numbers, strings,
                KeySelectorA: itemA => itemA,
                KeySelectorB: itemB => itemB,
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                AttributeSelectorA: null,
                AttributeSelectorB: null,
                AttributeComparer: null,
                KeySelfComparerA: (int i, int k) => i.CompareTo(k),
                KeySelfComparerB: (string i, string k) => String.Compare(i, k),
                checkSortOrder: true,
                OnCompared: (DIFF_STATE state, int num, string str) =>
                {
                    if ( state == DIFF_STATE.NEW)
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
            Diff.DiffSortedEnumerables<int, string, int, string, object, object>(
                Anumbers, Bstrings,
                KeySelectorA: itemA => itemA,
                KeySelectorB: itemB => itemB,
                KeyComparer: (numberA, stringB) => numberA.CompareTo(int.Parse(stringB)),
                AttributeSelectorA: null,
                AttributeSelectorB: null,
                AttributeComparer: null,
                KeySelfComparerA: (int i, int k) => i.CompareTo(k),
                KeySelfComparerB: (string i, string k) => String.Compare(i, k),
                checkSortOrder: true,
                OnCompared: (DIFF_STATE state, int num, string str) =>
                {
                    if (state == DIFF_STATE.NEW)
                    {
                        newItemsInB.Add(str);
                    }
                    else if (state == DIFF_STATE.DELETE)
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
            Diff.DiffSortedEnumerables<double, string, double, string, double, string>(
                Anumbers, Bstrings,
                KeySelectorA: itemA => itemA,
                KeySelectorB: itemB => itemB,
                KeyComparer: (numberA, stringB) =>
                {
                    int numberIntA = (int)(numberA);
                    int numberIntB = (int)(double.Parse(stringB));
                    return numberIntA.CompareTo(numberIntB);
                },
                AttributeSelectorA: itemA => itemA - Math.Floor(itemA),
                AttributeSelectorB: strB => 
                {
                    string[] parts = strB.Split(',');
                    return parts.Length == 2 ? parts[1] : String.Empty;
                },
                AttributeComparer: (double a, string b) =>
                {
                    double dblB = Convert.ToDouble("0," + b);
                    return a.CompareTo(dblB);
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
