using System;
using System.Collections.Generic;

namespace Spi
{
    public enum DIFF_STATE
    {
        NEW_B,
        MODIFY,
        DELETE_A,
        SAMESAME
    }
    public class DiffAscendingSortedLists
    {
        public static uint Run<A, B, C>(
            in IEnumerable<A> ListA,
            in IEnumerable<B> ListB,
            in Func<A, B, int> KeyComparerAB,
            in Func<A, B, int> AttributeComparer,
            in Func<A, A, int> KeyComparerA,
            in Func<B, B, int> KeyComparerB,
            in Action<DIFF_STATE, A, B, C> OnCompared,
            in bool checkSortOrder,
            in C context)
        {
            if (KeyComparerAB == null) throw new ArgumentNullException(nameof(KeyComparerAB));
            if (OnCompared    == null) throw new ArgumentNullException(nameof(OnCompared));

            if (checkSortOrder && (KeyComparerA == null || KeyComparerB == null))
            {
                throw new Exception("you want to check sortorder but there is no KeyComparerA or KeyComparerB specified");
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
                        DeltaState = DIFF_STATE.DELETE_A;
                        OnCompared(DeltaState, IterA.Current, default(B), context);
                        lastA = IterA.Current;
                        lastB = default(B);
                    }
                    else if (!hasMoreA && hasMoreB)
                    {
                        DeltaState = DIFF_STATE.NEW_B;
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
                        case DIFF_STATE.NEW_B:
                            hasMoreB = IterB.MoveNext();
                            break;
                        case DIFF_STATE.DELETE_A:
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
            in Func<A, B, int> keyComparer,
            in Func<A, B, int> attributeComparer,
            in A itemA,
            in B itemB)
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
                return keyCmpResult < 0 ? DIFF_STATE.DELETE_A : DIFF_STATE.NEW_B;
            }
        }
        private static void CheckSortOrderOfItems<K>(in Func<K, K, int> KeyComparer, in K lastItem, in K currItem, in char WhichList)
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
        #region OVERLOADING
        public static uint Run<T>(
            IEnumerable<T> ListA,
            IEnumerable<T> ListB,
            Comparison<T> KeyComparer,
            Action<DIFF_STATE, T, T> OnCompared,
            bool checkSortOrder)
        {
            return
                Run<T>(ListA, ListB,
                KeyComparer: KeyComparer,
                AttributeComparer: null,
                OnCompared: OnCompared,
                checkSortOrder: checkSortOrder);
        }
       
        public static uint Run<T>(
            IEnumerable<T>              ListA,
            IEnumerable<T>              ListB,
            Comparison<T>               KeyComparer,
            Comparison<T>               AttributeComparer,
            Action<DIFF_STATE, T, T>    OnCompared,
            bool                        checkSortOrder)
        {
            Func<T, T, int> KeySameTypeComparer = (T keyA, T keyB) => KeyComparer(keyA, keyB);

            Func<T, T, int> AttrSameTypeComparer;
            if ( AttributeComparer == null )
            {
                AttrSameTypeComparer = (Func<T, T, int>)null;
            }
            else
            {
                AttrSameTypeComparer = 
                    (T attrA, T attrB) => AttributeComparer(attrA, attrB);
            }

            return
                Run<T,T>(
                    ListA, ListB,
                    KeyComparer:        KeySameTypeComparer,
                    AttributeComparer:  AttrSameTypeComparer,
                    KeySelfComparerA:   KeySameTypeComparer,
                    KeySelfComparerB:   KeySameTypeComparer,
                    OnCompared:         OnCompared,
                    checkSortOrder:     checkSortOrder);
        }
        public static uint Run<A, B>(
            IEnumerable<A> ListA,
            IEnumerable<B> ListB,
            Func<A, B, int> KeyComparer,
            Func<A, B, int> AttributeComparer,
            Func<A, A, int> KeySelfComparerA,
            Func<B, B, int> KeySelfComparerB,
            Action<DIFF_STATE, A, B> OnCompared,
            bool checkSortOrder)
        {
            return
                DiffAscendingSortedLists.Run<A, B, object>(
                    ListA, 
                    ListB,
                    KeyComparerAB:      KeyComparer,
                    AttributeComparer:  AttributeComparer,
                    KeyComparerA:       KeySelfComparerA,
                    KeyComparerB:       KeySelfComparerB,
                    OnCompared:         (state, a, b, ctx) => OnCompared(state, a, b),
                    checkSortOrder:     checkSortOrder,
                    context:            null);
        }
        public static uint Run<A, B>(
            IEnumerable<A>  ListA,
            IEnumerable<B>  ListB,
            Func<A, B, int> KeyComparer,
            Func<A, B, int> AttributeComparer,
            Func<A, A, int> KeySelfComparerA,
            Func<B, B, int> KeySelfComparerB,
            Action<A>       OnDeleteA,
            Action<B>       OnNewB,
            Action<A, B>    OnModified,
            Action<A, B>    OnSameSame,
            bool checkSortOrder)
        {
            return
                DiffAscendingSortedLists.Run<A, B, object>(
                    ListA,
                    ListB,
                    KeyComparerAB:      KeyComparer,
                    AttributeComparer:  AttributeComparer,
                    KeyComparerA:       KeySelfComparerA,
                    KeyComparerB:       KeySelfComparerB,
                    OnCompared: (state, a, b, ctx) =>
                    {
                        switch (state)
                        {
                            case DIFF_STATE.SAMESAME: OnSameSame?.Invoke(a, b); break;
                            case DIFF_STATE.MODIFY:   OnModified?.Invoke(a, b); break;
                            case DIFF_STATE.NEW_B:    OnNewB?    .Invoke(b);    break;
                            case DIFF_STATE.DELETE_A: OnDeleteA? .Invoke(a);    break;
                        }
                    },
                    checkSortOrder: checkSortOrder,
                    context:        null);
        }
        #endregion
    }
}
