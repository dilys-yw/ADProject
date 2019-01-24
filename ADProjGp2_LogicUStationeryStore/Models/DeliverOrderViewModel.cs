using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class DeliverOrderViewModel
    {
        public string poNumber { get; set; }
        public string doNumber{ get; set; }
        public DateTime doDate { get; set; }
        public string status { get; set; }
        public string supplierID { get; set; }
        public string clerkID { get; set; }
        public DateTime orderDate { get; set; }
        public string orderRemark { get; set; }
        public string deliverRemark { get; set; }
        public List<DeliverOrderViewModelDetail> dovmdList { get; set; }
    }

    public class DeliverOrderViewModelDetail
    {
        public int podID { get; set; }
        public string itemNumber { get; set; }
        public string itemDescription { get; set; }
        public string itemUOM { get; set; }
        public int deliverQty { get; set; }
        public int purchaseQty { get; set; }
        public bool IsMatched { get; set; }
        public string remark { get; set; }
    }
}