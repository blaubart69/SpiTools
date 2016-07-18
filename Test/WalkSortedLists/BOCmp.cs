using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spi.Data;

namespace TestListDiff
{
    public class BOCmp : IComparable<BOCmp>
    {
        public string Name;
        public int Edition;

        private DIFF_COMPARE_RESULT DeltaTo(BOCmp other)
        {
            int cmp = this.Name.CompareTo(other.Name);
            if (cmp != 0)
            {
                return cmp < 0 ? DIFF_COMPARE_RESULT.LESS : DIFF_COMPARE_RESULT.GREATER;
            }
            else
            {
                int cmpEdt = this.Edition.CompareTo(other.Edition);
                return cmpEdt == 0 ? DIFF_COMPARE_RESULT.EQUAL : DIFF_COMPARE_RESULT.MODIFY;
            }
        }

        public int CompareTo(BOCmp other)
        {
            switch( this.DeltaTo(other) )
            {
                case DIFF_COMPARE_RESULT.EQUAL: return 0;
                case DIFF_COMPARE_RESULT.LESS: return -1;
                case DIFF_COMPARE_RESULT.GREATER: return 1;
                case DIFF_COMPARE_RESULT.MODIFY: return this.Edition.CompareTo(other.Edition);
            }
            throw new Exception("jezan is ollas aus");
        }
    }
}
