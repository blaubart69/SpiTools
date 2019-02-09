using System;
using System.Collections.Generic;

namespace Spi
{
    class DeltaCore
    {
        public static uint _internal_DiffSortedEnumerables<A, B, C>(
            IEnumerable<A> ListA,
            IEnumerable<B> ListB,
            Func<A, B, int> KeyComparerAB,
            Func<A, B, int> AttributeComparer,
            Func<A, A, int> KeyComparerA,
            Func<B, B, int> KeyComparerB,
            Action<DIFF_STATE, A, B, C> OnCompared,
            bool checkSortOrder,
            C context)
        {
            if (KeyComparerAB == null) throw new ArgumentNullException(nameof(KeyComparerAB));
            if (OnCompared == null) throw new ArgumentNullException(nameof(OnCompared));

            if (checkSortOrder && (KeyComparerA == null || KeyComparerB == null))
            {
                throw new Exception("you want to check sortorder but there is no KeySelfComparer specified");
            }
            
            using (IEnumerator<A> IterA = ListA.GetEnumerator())
            using (IEnumerator<B> IterB = ListB.GetEnumerator())
            {
                bool hasMoreA = IterA.MoveNext();
                bool hasMoreB = IterB.MoveNext();

                uint CountDifferences = 0;

                A lastA = default(A);
                B lastB = default(B);

                while (hasMoreA || hasMoreB)
                {
                    DIFF_STATE DeltaState;
                    #region delta_state
                    if (hasMoreA && hasMoreB)
                    {
                        DeltaState = ItemCompareFunc(KeyComparerAB, AttributeComparer, IterA.Current, IterB.Current);
                        OnCompared(DeltaState, IterA.Current, IterB.Current, context);
                        lastA = IterA.Current;
                        lastB = IterB.Current;
                    }
                    else if (hasMoreA && !hasMoreB)
                    {
                        DeltaState = DIFF_STATE.DELETE;
                        OnCompared(DeltaState, IterA.Current, default(B), context);
                        lastA = IterA.Current;
                        lastB = default(B);
                    }
                    else if (!hasMoreA && hasMoreB)
                    {
                        DeltaState = DIFF_STATE.NEW;
                        OnCompared(DeltaState, default(A), IterB.Current, context);
                        lastA = default(A);
                        lastB = IterB.Current;
                    }
                    else
                    {
                        throw new ApplicationException("internal state error. >>!hasMoreA || !hasMoreB<< should not be possible at this time");
                    }
                    #endregion
                    if (DeltaState != DIFF_STATE.SAMESAME)
                    {
                        CountDifferences += 1;
                    }
                    #region move_iterators
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
                    #endregion
                    #region check_sort_order
                    if (checkSortOrder)
                    {
                        // check if the sortorder is given and throw an exception if not
                        if (hasMoreA)
                        {
                            CheckSortOrderOfItems(KeyComparerA, lastA, IterA.Current, 'A');
                        }
                        if (hasMoreB)
                        {
                            CheckSortOrderOfItems(KeyComparerB, lastB, IterB.Current, 'B');
                        }
                    }
                    #endregion
                }
                return CountDifferences;
            }
        }

        private static DIFF_STATE ItemCompareFunc<A, B>(
            Func<A, B, int> keyComparer,
            Func<A, B, int> attributeComparer, 
            A itemA,
            B itemB)
        {
            int keyCmpResult = keyComparer(itemA, itemB);
            if (keyCmpResult == 0)
            {
                if (attributeComparer == null)
                {
                    return DIFF_STATE.SAMESAME;
                }

                if (attributeComparer(itemA, itemB) == 0)
                {
                    return DIFF_STATE.SAMESAME;
                }
                else
                {
                    return DIFF_STATE.MODIFY;
                }
            }
            else
            {
                return keyCmpResult < 0 ? DIFF_STATE.DELETE : DIFF_STATE.NEW;
            }
        }
        private static void CheckSortOrderOfItems<K>(Func<K, K, int> KeyComparer, K lastItem, K currItem, char WhichList)
        {
            if (KeyComparer(lastItem, currItem) > 0)
            {
                throw new ApplicationException(
                    String.Format(
                        "Sortorder not given in list [{0}]. Last item is greater than current item.\nlast [{1}]\ncurr [{2}]",
                        WhichList,
                        lastItem.ToString(),
                        currItem.ToString()));
            }
        }
    }
}
