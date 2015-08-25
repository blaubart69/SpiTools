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
            C                               contex
            )
        {
            var IterA = new DeluxeEnumerator<A>(ListA);
            var IterB = new DeluxeEnumerator<B>(ListB);

            IterA.MoveNext();   
            IterB.MoveNext();
            uint CountDifferences = 0;
            uint ItemsProcessed = 0;

            while (IterA.HasMoved || IterB.HasMoved)
            {
                DELTA_STATE DeltaState = DELTA_STATE.SAMESAME;
                if (IterA.HasMoved && IterB.HasMoved)
                {
                    DELTA_COMPARE_RESULT CmpResult = DeltaComparer(IterA.Current, IterB.Current);
                    DeltaState = GetDeltaStateFromCompareResult(CmpResult);
                    DoComparedCallback<A, B, C>(OnCompared, DeltaState, IterA.Current, IterB.Current, contex);
                    MoveIterators<A,B>(DeltaState, ref IterA, ref IterB);
                }
                else if (IterA.HasMoved && !IterB.HasMoved)
                {
                    DeltaState = DELTA_STATE.DELETE;
                    DoComparedCallback<A, B, C>(OnCompared, DeltaState, IterA.Current, default(B), contex);
                    IterA.MoveNext();
                }
                else if (!IterA.HasMoved && IterB.HasMoved)
                {
                    DeltaState = DELTA_STATE.NEW;
                    DoComparedCallback<A, B, C>(OnCompared, DeltaState, default(A), IterB.Current, contex);
                    IterB.MoveNext();
                }
                if (DeltaState != DELTA_STATE.SAMESAME)
                {
                    CountDifferences += 1;
                }
                ItemsProcessed += 1;
            }
            return CountDifferences;
        }
        private static void DoComparedCallback<A,B,C>(Action<DELTA_STATE,A,B,C> OnCompared, DELTA_STATE state, A a, B b, C contex)
        {
            if (OnCompared == null)
            {
                return;
            }
            OnCompared(state,a,b,contex);
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
        private static void MoveIterators<A,B>(DELTA_STATE state, ref DeluxeEnumerator<A> IterA, ref DeluxeEnumerator<B> IterB)
        {
            switch (state)
            {
                case DELTA_STATE.SAMESAME:
                case DELTA_STATE.MODIFY:
                    IterA.MoveNext();
                    IterB.MoveNext();
                    break;
                case DELTA_STATE.NEW:
                    IterB.MoveNext();
                    break;
                case DELTA_STATE.DELETE:
                    IterA.MoveNext();
                    break;
            }
        }
    }
}
