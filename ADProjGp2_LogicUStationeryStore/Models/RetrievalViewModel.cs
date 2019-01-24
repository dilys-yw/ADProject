using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class ReqListSummary
    {
        public string reqID { get; set; }
        public string deptName { get; set; }
        public string empName { get; set; }
        public string approveDate { get; set; }
        public bool IsSelected { get; set; }
    }

    public class RetListSummary
    {
        public string retID { get; set; }
        public string clerkName { get; set; }
        public string creationDate { get; set; }
        public string status { get; set; }
        public List<string> reqIDs { get; set; }
    }

    public class RetrievalListModel
    {
        public string requisitionID { get; set; }
        public string itemID { get; set; }
        public string itemName { get; set; }
        public string itemBin { get; set; }
        public string department  { get; set; }
        public int reqQty { get; set; }
        public int stockQty { get; set; }
        public Nullable<int> adjQty { get; set; }
        public int allocateQty { get; set; }
        public bool IsAdjusted { get; set; }
        public DateTime approvalDate { get; set; }
        public string remark { get; set; }
    }
    public class RetrievalListSummaryModel
    {
        public List<string> requisitionID { get; set; }
        public string itemID { get; set; }
        public string itemName { get; set; }
        public string department { get; set; }
        public string itemBin { get; set; }
        public int retrieveQty { get; set; }
        public int stockQty { get; set; }
        public int allocateQty { get; set; }
        public int tallyQty { get; set; }
    }

    public class InsufficientStockModel
    {
        public string requisitionID { get; set; }
        public string itemID { get; set; }
        public string itemName { get; set; }
        public int requestQty { get; set; }
        public int stockQty { get; set; }
    }

}