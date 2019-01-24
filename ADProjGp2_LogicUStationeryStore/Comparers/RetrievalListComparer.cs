using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class RetrievalListSumComparerByItemName : Comparer<RetrievalListSummaryModel>
    {
        public override int Compare(RetrievalListSummaryModel x, RetrievalListSummaryModel y)
        {
            int iteratingCount = 0;
            if (y.itemName.Length >= x.itemName.Length)
            {
                iteratingCount = x.itemName.Length;
            }
            else
            {
                iteratingCount = y.itemName.Length;
            }
            for (int i = 0; i < iteratingCount; i++)
            {
                if (x.itemName[i] > y.itemName[i])
                {
                    return 1;
                }
                else if (x.itemName[i] < y.itemName[i])
                {
                    return -1;
                }
                else if (x.itemName[i] == y.itemName[i])
                {
                    if (i == iteratingCount - 1)
                        return 0;
                }
            }
            return 0;
        }
    }

    public class RetListComparerByCreateDate : Comparer<RetListSummary>
    {
        public override int Compare(RetListSummary x, RetListSummary y)
        {
            bool trydate1 = DateTime.TryParse(x.creationDate, out DateTime date1);
            bool trydate2 = DateTime.TryParse(y.creationDate, out DateTime date2);
            if (trydate1 && trydate2)
            {
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
    public class RetrievalListComparerByAppDate : Comparer<RetrievalListModel>
    {
        public override int Compare(RetrievalListModel x, RetrievalListModel y)
        {
            if (x.approvalDate > y.approvalDate)
            {
                return 1;
            }
            else if (x.approvalDate == y.approvalDate)
            {
                return 0;
            }
            else if (x.approvalDate < y.approvalDate)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class RetrievalListComparerBySpecial : Comparer<RetrievalListModel>
    {
        public override int Compare(RetrievalListModel x, RetrievalListModel y)
        {
            if (x.requisitionID.Contains("Special") && !y.requisitionID.Contains("Special"))
            {
                return 1;
            }
            else if (x.requisitionID.Contains("Special") && y.requisitionID.Contains("Special"))
            {
                return 0;
            }
            else if (!x.requisitionID.Contains("Special") && y.requisitionID.Contains("Special"))
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class RetrievalListComparerByItemName : Comparer<RetrievalListModel>
    {
        public override int Compare(RetrievalListModel x, RetrievalListModel y)
        {
            int iteratingCount = 0;
            if (y.itemName.Length >= x.itemName.Length)
            {
                iteratingCount = x.itemName.Length;
            }
            else
            {
                iteratingCount = y.itemName.Length;
            }
            for (int i = 0; i < iteratingCount; i++)
            {
                if (x.itemName[i] > y.itemName[i])
                {
                    return 1;
                }
                else if (x.itemName[i] < y.itemName[i])
                {
                    return -1;
                }
                else if (x.itemName[i] == y.itemName[i])
                {
                    if(i == iteratingCount -1)
                    return 0;
                }
            }
            return 0;
        }
    }

    public class RetrievalListComparerByDepartmentID : Comparer<RetrievalListModel>
    {
        public override int Compare(RetrievalListModel x, RetrievalListModel y)
        {
            int iteratingCount = 0;
            if (y.itemName.Length >= x.itemName.Length)
            {
                iteratingCount = x.itemName.Length;
            }
            else
            {
                iteratingCount = y.itemName.Length;
            }
            for (int i = 0; i < iteratingCount; i++)
            {
                if (x.department[i] > y.department[i])
                {
                    return 1;
                }
                else if (x.department[i] < y.department[i])
                {
                    return -1;
                }
                else if (x.department[i] == y.department[i])
                {
                    if (i == iteratingCount - 1)
                        return 0;
                }
            }
            return 0;
        }
    }

}