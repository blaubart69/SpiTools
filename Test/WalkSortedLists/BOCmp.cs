using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spi;

namespace TestListDiff
{
    
    public class BOCmp 
    {
        public string Name;
        public int Edition;
        
        /*
        private DIFF_STATE DeltaTo(BOCmp other)
        {
            int cmp = this.Name.CompareTo(other.Name);
            if (cmp != 0)
            {
                return cmp < 0 ? DIFF_STATE. : DIFF_STATE.GREATER;
            }
            else
            {
                int cmpEdt = this.Edition.CompareTo(other.Edition);
                return cmpEdt == 0 ? DIFF_STATE.EQUAL : DIFF_STATE.MODIFY;
            }
        }
        
        public int CompareTo(BOCmp other)
        {
            int cmp = this.Name.CompareTo(other.Name);
            if (cmp != 0)
            {
                return cmp < 0 ? DIFF_STATE. : DIFF_STATE.GREATER;
            }
            else
            {
                int cmpEdt = this.Edition.CompareTo(other.Edition);
                return cmpEdt == 0 ? DIFF_STATE.EQUAL : DIFF_STATE.MODIFY;
            }

            switch ( this.DeltaTo(other) )
            {
                case DIFF_STATE.EQUAL: return 0;
                case DIFF_STATE.LESS: return -1;
                case DIFF_STATE.GREATER: return 1;
                case DIFF_STATE.MODIFY: return this.Edition.CompareTo(other.Edition);
            }
            throw new Exception("jezan is ollas aus");
        }
        */
    }
}
