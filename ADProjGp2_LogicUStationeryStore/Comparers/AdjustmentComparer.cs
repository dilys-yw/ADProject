using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class AdjustmentComparerVoucherDate : Comparer<AdjustmentVoucherViewModel>
    {
        public override int Compare(AdjustmentVoucherViewModel x, AdjustmentVoucherViewModel y)
        {
            if (x.voucherDate != null && y.voucherDate != null)
            {
                DateTime date1 = x.voucherDate;
                DateTime date2 = y.voucherDate;
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
                return 0;
            }

        }
    }
}