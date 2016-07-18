using System;
using System.Collections.Generic;

namespace Spi.Data
{
    public enum DIFF_COMPARE_RESULT
    {
        EQUAL,
        MODIFY,
        LESS,
        GREATER,
    }

    public enum DIFF_STATE
    {
        NEW,
        MODIFY,
        DELETE,
        SAMESAME
    }
    public class Diff
    {
        // ---------------------------------------------------------------------
        public static uint DiffSortedEnumerablesCheckSortorder<A, B>(
            IEnumerable<A> ListA,
            IEnumerable<B> ListB,
            Func<A, B, DIFF_COMPARE_RESULT> ItemCompareFunc,
            Action<DIFF_STATE, A, B> OnCompared)
        where A : IComparable<A>
        where B : IComparable<B>
        {
            return
                _internal_DiffSortedEnumerables<A, B, object>(
                    ListA: ListA,
                    ListB: ListB,
                    ItemCompareFunc: ItemCompareFunc,
                    OnCompared: (state, a, b, context) => OnCompared(state, a, b),
                    contex: null,
                    CompareToA: CompareTo,
                    CompareToB: CompareTo);
        }
        // ---------------------------------------------------------------------
        public static uint DiffSortedEnumerables<A, B>(
            IEnumerable<A>                  ListA, 
            IEnumerable<B>                  ListB, 
            Func<A,B,DIFF_COMPARE_RESULT>   ItemCompareFunc, 
            Action<DIFF_STATE,A,B>          OnCompared)
        {
            return 
                _internal_DiffSortedEnumerables<A, B, object>(
                    ListA: ListA,
                    ListB: ListB,
                    ItemCompareFunc: ItemCompareFunc,
                    OnCompared: (state, a, b, context) => OnCompared(state, a, b),
                    contex: null,
                    CompareToA: null,
                    CompareToB: null);
        }
        // ---------------------------------------------------------------------
        public static uint DiffSortedEnumerables<A, B, C>(
            IEnumerable<A> ListA,
            IEnumerable<B> ListB,
            Func<A, B, DIFF_COMPARE_RESULT> ItemCompareFunc,
            Action<DIFF_STATE, A, B, C> OnCompared,
            C contex)
        {
            return
                _internal_DiffSortedEnumerables<A, B, C>(
                    ListA: ListA,
                    ListB: ListB,
                    ItemCompareFunc: ItemCompareFunc,
                    OnCompared: (state, a, b, context) => OnCompared(state, a, b, contex),
                    contex: contex,
                    CompareToA: null,
                    CompareToB: null);
        }
        // ---------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A">Type of IEnumerable A</typeparam>
        /// <typeparam name="B">Type of IEnumerable B</typeparam>
        /// <typeparam name="C">Type of context</typeparam>
        /// <param name="ListA">First list</param>
        /// <param name="ListB">Second list</param>
        /// <param name="ItemCompareFunc">Function to compare two single items of the lists</param>
        /// <param name="OnCompared">Callback you get when two items has been compared</param>
        /// <param name="contex">contex that served in the callback function</param>
        /// <param name="CompareToA"></param>
        /// <param name="CompareToB"></param>
        /// <returns>number of different items</returns>
        private static uint _internal_DiffSortedEnumerables<A, B, C> (
            IEnumerable<A>                  ListA, 
            IEnumerable<B>                  ListB, 
            Func<A,B,DIFF_COMPARE_RESULT>   ItemCompareFunc, 
            Action<DIFF_STATE,A,B,C>        OnCompared,
            C                               contex,
            Func<A, A, int>                 CompareToA,
            Func<B, B, int>                 CompareToB)
        {
            if (ItemCompareFunc == null)  throw new ArgumentNullException("DeltaComparer");
            if (OnCompared      == null)  throw new ArgumentNullException("OnCompared");

            using (var IterA = ListA.GetEnumerator())
            using (var IterB = ListB.GetEnumerator())
            {
                bool hasMoreA = IterA.MoveNext();
                bool hasMoreB = IterB.MoveNext();

                uint CountDifferences = 0;

                A LastItemA = default(A);
                B LastItemB = default(B);

                while (hasMoreA || hasMoreB)
                {
                    DIFF_STATE DeltaState = DIFF_STATE.SAMESAME;
                    if (hasMoreA && hasMoreB)
                    {
                        DIFF_COMPARE_RESULT CmpResult = ItemCompareFunc(IterA.Current, IterB.Current);
                        DeltaState = GetDiffStateFromCompareResult(CmpResult);
                        OnCompared(DeltaState, IterA.Current, IterB.Current, contex);
                        LastItemA = IterA.Current;
                        LastItemB = IterB.Current;
                    }
                    else if (hasMoreA && !hasMoreB)
                    {
                        DeltaState = DIFF_STATE.DELETE;
                        OnCompared(DeltaState, IterA.Current, default(B), contex);
                        LastItemA = IterA.Current;
                        LastItemB = default(B);
                    }
                    else if (!hasMoreA && hasMoreB)
                    {
                        DeltaState = DIFF_STATE.NEW;
                        OnCompared(DeltaState, default(A), IterB.Current, contex);
                        LastItemA = default(A);
                        LastItemB = IterB.Current;
                    }

                    if (DeltaState != DIFF_STATE.SAMESAME)
                    {
                        CountDifferences += 1;
                    }
                    // move the iterators based on the diff result
                    switch (DeltaState)
                    {
                        case DIFF_STATE.SAMESAME:
                        case DIFF_STATE.MODIFY:
                            hasMoreA = IterA.MoveNext();
                            hasMoreB = IterB.MoveNext();
                            break;
                        case DIFF_STATE.NEW:
                            hasMoreB = IterB.MoveNext();
                            break;
                        case DIFF_STATE.DELETE:
                            hasMoreA = IterA.MoveNext();
                            break;
                    }
                    // check if the sortorder is given and throw an exception if not
                    if (hasMoreA && CompareToA != null)
                    {
                        CheckSortOrderOfItems(CompareToA, LastItemA, IterA.Current, 'A');
                    }
                    if (hasMoreB && CompareToB != null)
                    {
                        CheckSortOrderOfItems(CompareToB, LastItemB, IterB.Current, 'B');
                    }
                }
                return CountDifferences;
            }
        }
        private static DIFF_STATE GetDiffStateFromCompareResult(DIFF_COMPARE_RESULT CmpResult)
        {
            DIFF_STATE DiffState;

            switch (CmpResult)
            {
                case DIFF_COMPARE_RESULT.EQUAL:
                    DiffState = DIFF_STATE.SAMESAME;
                    break;
                case DIFF_COMPARE_RESULT.MODIFY:
                    DiffState = DIFF_STATE.MODIFY;
                    break;
                case DIFF_COMPARE_RESULT.LESS:
                    DiffState = DIFF_STATE.DELETE;
                    break;
                case DIFF_COMPARE_RESULT.GREATER:
                    DiffState = DIFF_STATE.NEW;
                    break;
                default:
                    DiffState = DIFF_STATE.SAMESAME;
                    break;
            }

            return DiffState;
        }
        private static void CheckSortOrderOfItems<T>(Func<T,T,int> CompareTo, T lastItem, T currentItem, char WhichList)
        {
            if (CompareTo(lastItem, currentItem) > 0)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Sortorder not given in list [{0}]. Last item is greater than current item."
                     + " Last [{1}] > [{2}] (current)",
                        WhichList,
                        lastItem.ToString(),
                        currentItem.ToString()));
            }
        }
        private static int CompareTo<T>(T a, T b)
            where T : IComparable<T>
        {
            return a.CompareTo(b);
        }
    }
}
