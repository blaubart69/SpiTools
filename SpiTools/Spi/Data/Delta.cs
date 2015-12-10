using System;
using System.Collections.Generic;

namespace Spi.Data
{
    public enum DELTA_COMPARE_RESULT
    {
        EQUAL,
        MODIFY,
        LESS,
        GREATER,
    }

    public enum DELTA_STATE
    {
        NEW,
        MODIFY,
        DELETE,
        SAMESAME
    }
    public class Delta
    {
        public static uint WalkSortedLists<A, B>(
            IEnumerable<A>                  ListA, 
            IEnumerable<B>                  ListB, 
            Func<A,B,DELTA_COMPARE_RESULT>  DeltaComparer, 
            Action<DELTA_STATE,A,B>         OnCompared)
        {
            return WalkSortedLists<A, B, object>(
                ListA, 
                ListB, 
                DeltaComparer,
                (state, a, b, context) => OnCompared(state, a, b),
                null);
        }

        public static uint WalkSortedLists<A, B, C>(
            IEnumerable<A>                  ListA, 
            IEnumerable<B>                  ListB, 
            Func<A,B,DELTA_COMPARE_RESULT>  DeltaComparer, 
            Action<DELTA_STATE,A,B,C>       OnCompared,
            C                               contex)
        {
            if (DeltaComparer   == null)  throw new ArgumentNullException("DeltaComparer");
            if (OnCompared      == null)  throw new ArgumentNullException("OnCompared");

            var IterA = ListA.GetEnumerator();
            var IterB = ListB.GetEnumerator();

            bool hasMoreA = IterA.MoveNext();   
            bool hasMoreB = IterB.MoveNext();
            
            uint CountDifferences = 0;
            uint ItemsProcessed = 0;

            while (hasMoreA || hasMoreB)
            {
                DELTA_STATE DeltaState = DELTA_STATE.SAMESAME;
                if (hasMoreA && hasMoreB)
                {
                    DELTA_COMPARE_RESULT CmpResult = DeltaComparer(IterA.Current, IterB.Current);
                    DeltaState = GetDeltaStateFromCompareResult(CmpResult);
                    OnCompared(DeltaState, IterA.Current, IterB.Current, contex);
                }
                else if (hasMoreA && !hasMoreB)
                {
                    DeltaState = DELTA_STATE.DELETE;
                    OnCompared(DeltaState, IterA.Current, default(B), contex);
                }
                else if (!hasMoreA && hasMoreB)
                {
                    DeltaState = DELTA_STATE.NEW;
                    OnCompared(DeltaState, default(A), IterB.Current, contex);
                }

                if (DeltaState != DELTA_STATE.SAMESAME)
                {
                    CountDifferences += 1;
                }

                ItemsProcessed += 1;

                switch (DeltaState)
                {
                    case DELTA_STATE.SAMESAME:
                    case DELTA_STATE.MODIFY:
                        hasMoreA = IterA.MoveNext();
                        hasMoreB = IterB.MoveNext();
                        break;
                    case DELTA_STATE.NEW:
                        hasMoreB = IterB.MoveNext();
                        break;
                    case DELTA_STATE.DELETE:
                        hasMoreA = IterA.MoveNext();
                        break;
                }
            }
            return CountDifferences;
        }
        private static DELTA_STATE GetDeltaStateFromCompareResult(DELTA_COMPARE_RESULT CmpResult)
        {
            DELTA_STATE DeltaState;// = DELTA_STATE.SAMESAME;

            switch (CmpResult)
            {
                case DELTA_COMPARE_RESULT.EQUAL:
                    DeltaState = DELTA_STATE.SAMESAME;
                    break;
                case DELTA_COMPARE_RESULT.MODIFY:
                    DeltaState = DELTA_STATE.MODIFY;
                    break;
                case DELTA_COMPARE_RESULT.LESS:
                    DeltaState = DELTA_STATE.DELETE;
                    break;
                case DELTA_COMPARE_RESULT.GREATER:
                    DeltaState = DELTA_STATE.NEW;
                    break;
                default:
                    DeltaState = DELTA_STATE.SAMESAME;
                    break;
            }

            return DeltaState;
        }
    }
}
