using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class ClerkWelcomePageModel
    {
        public List<RequisitionWelcomePageComponent> RequisitionWPC { get; set; }
        public List<RetrievalWelcomePageComponent> RetrievalWPC { get; set; }
        public List<DisbursementWelcomePageComponent> DisbursementWPC { get; set; }
        public List<AdjustmentWelcomePageComponent> AdjustmentWPC { get; set; }
        public List<StockWarningComponent> StockWPC { get; set; }
    }

    public class SupervisorWelcomePageModel
    {
        public List<AdjustmentWelcomePageComponent> AdjustmentWPC { get; set; }
    }

    public class ManagerWelcomePageModel
    {
        public List<AdjustmentWelcomePageComponent> AdjustmentWPC { get; set; }
    }

    public class AdjustmentWelcomePageComponent
    {
        public string AdjustmentID { get; set; }
        public string AdjustmentDate { get; set; }
        public string AdjustmentCreator { get; set; }
        public string AdjustmentStatus { get; set; }

    }
    public class RequisitionWelcomePageComponent
    {
        public string RequisitionID { get; set; }
        public string RequisitionApproveDate { get; set; }
        public string RequisitionCreator { get; set; }
        public string RequisitionStatus { get; set; }

    }

    public class RetrievalWelcomePageComponent
    {
        public string RetrievalID { get; set; }
        public string RetrievalCreationDate { get; set; }
        public string RetrievalCreator { get; set; }
        public string RetrievalStatus { get; set; }
    }

    public class StockWarningComponent
    {
        public string ItemID{ get; set; }
        public string ItemDescription { get; set; }
        public int qtyStock { get; set; }
        public int qtyReorderLevel { get; set; }
        public bool purchaseMade{ get; set; }
        public string PurchaseOrderID { get; set; }
        public int purchaseQty { get; set; }
    }

    public class DisbursementWelcomePageComponent
    {
        public string DisbursementID { get; set; }
        public string DisbursementCreationDate { get; set; }
        public string DisbursementCollectDate { get; set; }
        public string DisbursementCreator { get; set; }
        public string DisbursementStatus { get; set; }
        public string DisburementCollectionPoint { get; set; }
        public string DisbursementRep { get; set; }
        public string DisbursementDept { get; set; }
        public string DisbursementRetID { get; set; }
    }
}