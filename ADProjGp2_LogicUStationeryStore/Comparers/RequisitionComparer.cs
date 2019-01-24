using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class RequisitionComparerApprovalDate : Comparer<Requisition>
    {
        public override int Compare(Requisition x, Requisition y)
        {
            if (x.approvalDate > y.approvalDate)
            {
                return -1;
            }
            else if (x.approvalDate == y.approvalDate)
            {
                return 0;
            }
            else if (x.approvalDate < y.approvalDate)
            {
                return 1;
            }
            else
            {
                return 1;
            }
        }
    }
}