using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class StockTakeModel
    {
        public string binNumber { get; set; }
        public string stocktakeDate { get; set; }
        public string stocktakeClerk { get; set; }
        public List<StockTakeModelDetails> stmdList {get;set;}
    }
    public class StockTakeModelDetails
    {
        public string itemID { get; set; }
        public string itemDescription { get; set; }
        public string catagory { get; set; }

        public int invQty { get; set; }
        public int disburseQty { get; set; }
        public int adjQty { get; set; }
        public int UserAdjQty { get; set; }
    }
}