using System;
using System.Collections.Generic;

namespace Spi
{
    class DeltaCore
    {
        public static uint _internal_DiffSortedEnumerables<TA, TB, KA, KB, AA, AB, C>(
            IEnumerable<TA> ListA,
            IEnumerable<TB> ListB,
            Func<TA, KA> KeySelector1,
            Func<TB, KB> KeySelector2,
            Func<KA, KB, int> KeyComparer,
            Func<TA, AA> AttributeSelector1,
            Func<TB, AB> AttributeSelector2,
            Func<AA, AB, int> AttributeComparer,
            Func<KA, KA, int> KeySelfComparer1,
            Func<KB, KB, int> KeySelfComparer2,
            Action<DIFF_STATE, TA, TB, C> OnCompared,
            bool checkSortOrder,
            C context)
        {
            if (KeySelector1 == null) throw new ArgumentNullException(nameof(KeySelector1));
            if (KeySelector2 == null) throw new ArgumentNullException(nameof(KeySelector2));
            if (KeyComparer == null) throw new ArgumentNullException(nameof(KeyComparer));
            if (OnCompared == null) throw new ArgumentNullException(nameof(OnCompared));

            if (checkSortOrder && (KeySelfComparer1 == null || KeySelfComparer2 == null))
            {
                throw new Exception("you want to check sortorder but there is no KeySelfComparer specified");
            }
            
            using (IEnumerator<TA> IterA = ListA.GetEnumerator())
            using (IEnumerator<TB> IterB = ListB.GetEnumerator())
            {
                bool hasMoreA = IterA.MoveNext();
                bool hasMoreB = IterB.MoveNext();

                uint CountDifferences = 0;

                KA LastKeyA = default(KA);
                KB LastKeyB = default(KB);
                KA keyA = hasMoreA ? KeySelector1(IterA.Current) : default(KA);
                KB keyB = hasMoreB ? KeySelector2(IterB.Current) : default(KB);

                while (hasMoreA || hasMoreB)
                {
                    DIFF_STATE DeltaState = DIFF_STATE.SAMESAME;
                    if (hasMoreA && hasMoreB)
                    {
                        DeltaState = ItemCompareFunc(
                            KeyComparer(keyA, keyB),
                            IterA.Current, IterB.Current,
                            AttributeSelector1, AttributeSelector2, AttributeComparer);
                        OnCompared(DeltaState, IterA.Current, IterB.Current, context);
                        LastKeyA = keyA;
                        LastKeyB = keyB;
                    }
                    else if (hasMoreA && !hasMoreB)
                    {
                        DeltaState = DIFF_STATE.DELETE;
                        OnCompared(DeltaState, IterA.Current, default(TB), context);
                        LastKeyA = keyA;
                        LastKeyB = default(KB);
                    }
                    else if (!hasMoreA && hasMoreB)
                    {
                        DeltaState = DIFF_STATE.NEW;
                        OnCompared(DeltaState, default(TA), IterB.Current, context);
                        LastKeyA = default(KA);
                        LastKeyB = keyB;
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
                    if (checkSortOrder)
                    {
                        // check if the sortorder is given and throw an exception if not
                        if (hasMoreA)
                        {
                            keyA = KeySelector1(IterA.Current);
                            CheckSortOrderOfItems(KeySelfComparer1, LastKeyA, keyA, 'A');
                        }
                        if (hasMoreB)
                        {
                            keyB = KeySelector2(IterB.Current);
                            CheckSortOrderOfItems(KeySelfComparer2, LastKeyB, keyB, 'B');
                        }
                    }
                }
                return CountDifferences;
            }
        }

        private static DIFF_STATE ItemCompareFunc<T1, T2, A1, A2>(
            int KeyCmpResult,
            T1 itemA, T2 itemB,
            Func<T1, A1> attributeSelector1,
            Func<T2, A2> attributeSelector2,
            Func<A1, A2, int> attributeComparer)
        {
            if (KeyCmpResult == 0)
            {
                if (attributeSelector1 == null || attributeSelector2 == null)
                {
                    return DIFF_STATE.SAMESAME;
                }

                A1 attrA = attributeSelector1(itemA);
                A2 attrB = attributeSelector2(itemB);

                if (attributeComparer(attrA, attrB) == 0)
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
                return KeyCmpResult < 0 ? DIFF_STATE.DELETE : DIFF_STATE.NEW;
            }
        }
        private static void CheckSortOrderOfItems<K>(Func<K, K, int> KeyComparer, K lastKey, K currentKey, char WhichList)
        {
            if (KeyComparer(lastKey, currentKey) > 0)
            {
                throw new ApplicationException(
                    String.Format(
                        "Sortorder not given in list [{0}]. Last item is greater than current item.\nlast [{1}]\ncurr [{2}]",
                        WhichList,
                        lastKey.ToString(),
                        currentKey.ToString()));
            }
        }
    }
}
