using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class PurchaseOrderViewModel
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public int SQuantity { get; set; }
        public List<string> Suppliers { get; set; }
        public string SelectedSupplier { get; set; }
        public IList<SelectListItem> ItemSup { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public string PoId { get; set; }
    }

    public class SuggestionViewModel
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public int SQuantity { get; set; }
        public bool IsSelected { get; set; }
        public string remark { get; set; }
    }

    public class CustomPOViewModel
    {
        public IList<PurchaseOrderViewModel> Pvm { get; set; }
        public Dictionary<string, string> PoList { get; set; }
    }

}