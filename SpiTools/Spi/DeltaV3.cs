using System;
using System.Collections.Generic;

namespace Europc.WAM.SyncPG
{
    
    public static class DiffAscendingSortedLists
    {
        private enum DIFF_STATE
        {
            NEW_B,
            MODIFY,
            DELETE_A,
            SAMESAME
        }

        public delegate int  Comparison       <A, B>(A objA, B objB);
        public delegate bool AttributeComparer<A, B, D>(A objA, B objB, out D AttrCmpResult);

        public static void Run<A, B, D>(
            in IEnumerable<A>                   ListA,
            in IEnumerable<B>                   ListB,
            in Comparison<A, B>                 KeyComparerAB,
            in AttributeComparer<A, B, D>       AttributeComparer,
            in Action<B>                        OnNewB,
            in Action<A>                        OnDeleteA,
            in Action<A, B, D>                  OnModified,
            in Action<A, B>                     OnSameSame,
            in Comparison<A>                    KeySortOrderComparerA,
            in Comparison<B>                    KeySortOrderComparerB)
        {
            if (KeyComparerAB == null) throw new ArgumentNullException(nameof(KeyComparerAB));

            using (IEnumerator<A> IterA = ListA.GetEnumerator())
            using (IEnumerator<B> IterB = ListB.GetEnumerator())
            {
                bool hasMoreA = IterA.MoveNext();
                bool hasMoreB = IterB.MoveNext();

                A lastA = default;
                B lastB = default;

                while (hasMoreA || hasMoreB)
                {
                    DIFF_STATE DeltaState;
                    D attribCompareResult = default;
                    #region DELTA_STATE__LASTA__LASTB
                    if (hasMoreA && hasMoreB)
                    {
                        DeltaState = ItemCompareFunc(KeyComparerAB, AttributeComparer, IterA.Current, IterB.Current, out attribCompareResult);
                        lastA = IterA.Current;
                        lastB = IterB.Current;
                    }
                    else if (hasMoreA && !hasMoreB)
                    {
                        DeltaState = DIFF_STATE.DELETE_A;
                        lastA = IterA.Current;
                        lastB = default;
                    }
                    else if (!hasMoreA && hasMoreB)
                    {
                        DeltaState = DIFF_STATE.NEW_B;
                        lastA = default;
                        lastB = IterB.Current;
                    }
                    else
                    {
                        throw new ApplicationException("internal state error. >>!hasMoreA || !hasMoreB<< should not be possible at this time");
                    }
                    #endregion
                    #region CALL_THE_CALLBACKS
                    switch (DeltaState)
                    {
                        case DIFF_STATE.NEW_B:      OnNewB?    .Invoke(IterB.Current);                                      break;
                        case DIFF_STATE.DELETE_A:   OnDeleteA? .Invoke(IterA.Current);                                      break;
                        case DIFF_STATE.MODIFY:     OnModified?.Invoke(IterA.Current, IterB.Current, attribCompareResult);  break;
                        case DIFF_STATE.SAMESAME:   OnSameSame?.Invoke(IterA.Current, IterB.Current);                       break;
                    }
                    #endregion
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
                    // check if the sortorder is given and throw an exception if not
                    if (KeySortOrderComparerA != null &&hasMoreA)
                    {
                        CheckSortOrderOfItems(KeySortOrderComparerA, lastA, IterA.Current, 'A');
                    }
                    if (KeySortOrderComparerA != null && hasMoreB)
                    {
                        CheckSortOrderOfItems(KeySortOrderComparerB, lastB, IterB.Current, 'B');
                    }
                    #endregion
                }
            }
        }
        private static DIFF_STATE ItemCompareFunc<A,B,D>(
            in Comparison<A,B>                  keyComparer,
            in AttributeComparer<A,B,D>         attribComparer,
            in A                                itemA,
            in B                                itemB,
            out D                               attribCompareResult)
        {
            attribCompareResult = default;

            int keyCmpResult = keyComparer(itemA, itemB);
            if (keyCmpResult == 0)
            {
                if (attribComparer == null)
                {
                    return DIFF_STATE.SAMESAME;
                }

                bool attributesEqual = attribComparer(itemA, itemB, out attribCompareResult);
                return attributesEqual ? DIFF_STATE.SAMESAME : DIFF_STATE.MODIFY;
            }
            else
            {
                return keyCmpResult < 0 ? DIFF_STATE.DELETE_A : DIFF_STATE.NEW_B;
            }
        }
        private static void CheckSortOrderOfItems<K>(
            in Comparison<K>    KeyComparer, 
            in K                lastItem, 
            in K                currItem, 
            in char             WhichList)
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
        //
        // ====================================================================
        //
        #region OVERLOADING_SAME_TYPE
        public static void RunSameType<T,D>(
            IEnumerable<T>      ListA,
            IEnumerable<T>      ListB,
            Comparison<T>       KeyComparer,
            AttributeComparer<T, T, D> AttributeComparer,
            Action<T>           OnDeleteA,
            Action<T>           OnNewB,
            Action<T, T, D>     OnModified,
            Action<T, T>        OnSameSame,
            bool                checkSortOrder)
        {
            DiffAscendingSortedLists.Comparison<T, T> KeySameTypeComparer   = (T keyA, T keyB)  => KeyComparer(keyA, keyB);

            System.Comparison<T> keySortOrderComparer = checkSortOrder ? (T a, T b) => KeyComparer(a, b) : (System.Comparison<T>)null;

            AttributeComparer<T, T, D> AttrSameTypeComparer;
            if (AttributeComparer == null)
            {
                AttrSameTypeComparer = null;
            }
            else
            {
                AttrSameTypeComparer =
                    (T attrA, T attrB, out D AttrCmpResult) =>
                    {
                        return AttributeComparer(attrA, attrB, out AttrCmpResult);
                    };
            }

            DiffAscendingSortedLists.Run<T, T, D>(
                ListA,
                ListB,
                KeySameTypeComparer,
                AttrSameTypeComparer,
                OnNewB,
                OnDeleteA,
                OnModified,
                OnSameSame,
                KeySortOrderComparerA: keySortOrderComparer,
                KeySortOrderComparerB: keySortOrderComparer);
        }
        public static void NoModified<T>(
            in IEnumerable<T>       ListA,
            in IEnumerable<T>       ListB,
               System.Comparison<T> KeyComparer,
            in Action<T>            OnDeleteA,
            in Action<T>            OnNewB,
            in Action<T, T>         OnSameSame)
        {
            DiffAscendingSortedLists.Comparison<T, T> KeySameTypeComparer = (T keyA, T keyB) => KeyComparer(keyA, keyB);

            NoModified<T,T>(ListA, ListB, KeySameTypeComparer, OnDeleteA, OnNewB, OnSameSame, KeyComparer, KeyComparer);
        }
        #endregion
        #region OVERLOADING_TWO_TYPES
        public static void NoModified<A,B>(
            in IEnumerable<A>       ListA,
            in IEnumerable<B>       ListB,
            in Comparison<A, B>     KeyComparer,
            in Action<A>            OnDeleteA,
            in Action<B>            OnNewB,
            in Action<A, B>         OnSameSame,
            in Comparison<A>        KeySortOrderComparerA,
            in Comparison<B>        KeySortOrderComparerB)
        {
            Run<A,B,object>(ListA, ListB, KeyComparer,
                AttributeComparer: null,
                OnDeleteA:    OnDeleteA,
                OnNewB:       OnNewB,
                OnSameSame:   OnSameSame,
                OnModified: (a,b,attrResult) => 
                    throw new Exception("DiffAscendingSortedLists(NoModified)/internal error: as there is no AttributeComparer there should be no OnMofidied"),
                KeySortOrderComparerA: KeySortOrderComparerA,
                KeySortOrderComparerB: KeySortOrderComparerB );
        }
        #endregion
    }
}
