using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class AdjustmentVoucherViewModel
    {
        public string voucherID { get; set; }
        public DateTime voucherDate { get; set; }
        public string clerkIDName { get; set; }
        public string supervisorIDName { get; set; }
        public string managerIDName { get; set; }
        public bool needAuthority { get; set; }
        public string status { get; set; }
        public decimal? adjustmentValue { get; set; }
        public List<AdjustmentVoucherViewModelDetail> itemList { get; set; }
    }

    public class AdjustmentVoucherViewModelDetail
    {
        public string itemID { get; set; }
        public string itemDescription { get; set; }
        public int itemQty { get; set; }
        public int itemStoreQty { get; set; }
        public int itemDisburseQty { get; set; }
        public int itemInventoryLocation { get; set; }
        public string remark { get; set; }
    }
}