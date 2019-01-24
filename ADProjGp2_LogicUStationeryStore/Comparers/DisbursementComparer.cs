using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class DisbursementComparerDisDate : Comparer<DisbursementHistoryViewModel>
    {
        public override int Compare(DisbursementHistoryViewModel x, DisbursementHistoryViewModel y)
        {
            if (x.disburseDate != null && y.disburseDate != null)
            {
                DateTime date1 = (DateTime)x.disburseDate;
                DateTime date2 = (DateTime)y.disburseDate;
                if (date1 > date2)
                {
                    return -1;
                }
                else if (date1 == date2)
                {
                    return 0;
                }
                else if (date1 < date2)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }

        }
    }
}