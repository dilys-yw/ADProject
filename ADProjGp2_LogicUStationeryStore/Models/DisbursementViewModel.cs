using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class DisbursementHistoryViewModel
    {
        public string retrievalID { get; set; }
        public DateTime? disburseDate { get; set; }
        public List<string> disbursementIDs{ get; set; }
    }
    public class DisbursementViewModel
    {        
        public string disbursementID { get; set; }
        public string collectionPoint { get; set; }
        public string collectionTime { get; set; }
        public DateTime? disburseDate { get; set; }
        public string requestorDeptID { get; set; }
        public string repName { get; set; }
        public List<DisbursementViewModelDetail> dvmdList { get; set; }
        public string status { get; set; }
        public string requestorName { get; set; }
        public string deptID { get; set; }
        public string adjustmentID { get; set; }
        
    }

    public class DisbursementViewModelDetail
    {
        public string retrievalID { get; set; }
        public string requestorName { get; set; }
        public string requestorID { get; set; }
        public string requisitionID { get; set; }
        public string itemID { get; set; }
        public string itemDescription { get; set; }
        public int? retrieveQty { get; set; }
        public int requestQty { get; set; }
        public int? adjQty { get; set; }
    }
}