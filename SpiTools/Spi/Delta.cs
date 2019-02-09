using System;
using System.Collections.Generic;

namespace Spi
{
    public enum DIFF_STATE
    {
        NEW,
        MODIFY,
        DELETE,
        SAMESAME
    }
    public class Diff
    {
        public static uint DiffSortedEnumerables<T>(
            IEnumerable<T> ListA,
            IEnumerable<T> ListB,
            Comparison<T> KeyComparer,
            Action<DIFF_STATE, T, T> OnCompared,
            bool checkSortOrder)
        {
            return
                DiffSortedEnumerables<T>(ListA, ListB,
                KeyComparer: KeyComparer,
                AttributeComparer: null,
                OnCompared: OnCompared,
                checkSortOrder: checkSortOrder);
        }
       
        public static uint DiffSortedEnumerables<T>(
            IEnumerable<T>              ListA,
            IEnumerable<T>              ListB,
            Comparison<T>               KeyComparer,
            Comparison<T>               AttributeComparer,
            Action<DIFF_STATE, T, T>    OnCompared,
            bool                        checkSortOrder)
        {
            Func<T, T, int> KeySameTypeComparer = (T keyA, T keyB) => KeyComparer(keyA, keyB);

            Func<T, T, int> attrComparerToUse;
            if ( AttributeComparer == null )
            {
                attrComparerToUse = (Func<T, T, int>)null;
            }
            else
            {
                attrComparerToUse = (T attrA, T attrB) => AttributeComparer(attrA, attrB);
            }

            return
                DiffSortedEnumerables<T,T>(
                    ListA, ListB,
                    KeyComparer:        KeySameTypeComparer,
                    AttributeComparer:  attrComparerToUse,
                    KeySelfComparerA:   KeySameTypeComparer,
                    KeySelfComparerB:   KeySameTypeComparer,
                    OnCompared:         OnCompared,
                    checkSortOrder:     checkSortOrder);
        }
        public static uint DiffSortedEnumerables<A, B>(
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
                DeltaCore._internal_DiffSortedEnumerables<A, B, object>(
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
    }
}
