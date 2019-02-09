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
            IEnumerable<T> ListA,
            IEnumerable<T> ListB,
            Comparison<T>  KeyComparer,
            Comparison<T>  AttributeComparer,
            Action<DIFF_STATE, T, T> OnCompared,
            bool checkSortOrder)
        {
            return
                DiffSortedEnumerables<T, T, T>(ListA, ListB,
                KeySelector:        item => item,
                KeyComparer:        KeyComparer,
                AttributeSelector:  item => item,
                AttributeComparer:  AttributeComparer,
                OnCompared:         OnCompared,
                checkSortOrder:     checkSortOrder);
        }
        public static uint DiffSortedEnumerables<T, K>(
            IEnumerable<T>              ListA,
            IEnumerable<T>              ListB,
            Func<T, K>                  KeySelector,
            Comparison<K>               KeyComparer,
            Action<DIFF_STATE, T, T>    OnCompared,
            bool                        checkSortOrder)
        {
            return
                DiffSortedEnumerables<T, K, object>(
                    ListA,
                    ListB,
                    KeySelector,
                    KeyComparer,
                    AttributeSelector: null,
                    AttributeComparer: null,
                    OnCompared: OnCompared,
                    checkSortOrder: checkSortOrder);
        }
        public static uint DiffSortedEnumerables<T, K, A>(
            IEnumerable<T>              ListA,
            IEnumerable<T>              ListB,
            Func<T, K>                  KeySelector,
            Comparison<K>               KeyComparer,
            Func<T, A>                  AttributeSelector,
            Comparison<A>               AttributeComparer,
            Action<DIFF_STATE, T, T>    OnCompared,
            bool                        checkSortOrder)
        {
            Func<K, K, int> KeySameTypeComparer = (K keyA, K keyB) => KeyComparer(keyA, keyB);

            return
                DiffSortedEnumerables<T, T, K, K, A, A>(
                    ListA, ListB,
                    KeySelectorA:       KeySelector,
                    KeySelectorB:       KeySelector,
                    KeyComparer:        KeySameTypeComparer,
                    AttributeSelectorA: AttributeSelector,
                    AttributeSelectorB: AttributeSelector,
                    //AttributeComparer:  (A attrA, A attrB) => AttributeComparer(attrA, attrB),
                    AttributeComparer:  AttributeComparer == null ? (Func<A, A, int>)null : (A attrA, A attrB) => AttributeComparer(attrA, attrB),
                    KeySelfComparerA:   KeySameTypeComparer,
                    KeySelfComparerB:   KeySameTypeComparer,
                    OnCompared:         OnCompared,
                    checkSortOrder:     checkSortOrder);
        }
        public static uint DiffSortedEnumerables<TA, TB, KA, KB, AA, AB>(
            IEnumerable<TA> ListA,
            IEnumerable<TB> ListB,
            Func<TA, KA> KeySelectorA,
            Func<TB, KB> KeySelectorB,
            Func<KA, KB, int> KeyComparer,
            Func<TA, AA> AttributeSelectorA,
            Func<TB, AB> AttributeSelectorB,
            Func<AA, AB, int> AttributeComparer,
            Func<KA, KA, int> KeySelfComparerA,
            Func<KB, KB, int> KeySelfComparerB,
            Action<DIFF_STATE, TA, TB> OnCompared,
            bool checkSortOrder)
        {
            return
            DeltaCore._internal_DiffSortedEnumerables<TA, TB, KA, KB, AA, AB, object>(
                ListA, ListB,
                KeySelector1:       KeySelectorA,
                KeySelector2:       KeySelectorB,
                KeyComparer:        KeyComparer,
                AttributeSelector1: AttributeSelectorA,
                AttributeSelector2: AttributeSelectorB,
                AttributeComparer:  AttributeComparer,
                KeySelfComparer1:   KeySelfComparerA,
                KeySelfComparer2:   KeySelfComparerB,
                OnCompared:         (state, a, b, ctx) => OnCompared(state, a, b),
                checkSortOrder:     checkSortOrder,
                context:            null);

        }
    }
}
