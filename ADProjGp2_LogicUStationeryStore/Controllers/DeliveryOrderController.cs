using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class DeliveryOrderController : Controller
    {
        DeliveryOrderBusinessLogic deliverBizLogic = new DeliveryOrderBusinessLogic();

        [HttpPost]
        public ActionResult DeliveryOrderEntry(string PONumber)
        {
            DeliverOrderViewModel dovm = deliverBizLogic.DeliveryOrderEntryPreparation(PONumber);
            return View(dovm);
        }

        [HttpPost]
        public ActionResult DeliveryOrderEntryConfirm(DeliverOrderViewModel dovm)
        {          
            return View(dovm);
        }

        [HttpPost]
        public ActionResult DeliveryOrderEntrySubmission(DeliverOrderViewModel dovm)
        {
            dovm = deliverBizLogic.DeliverOrderMatching(dovm);
            deliverBizLogic.DeliveryConfirmation(dovm);
            return RedirectToAction("PurchaseOrderList", "Purchase");
        }

        public ActionResult DeliveryOrderList()
        {
            SSISEntities db = new SSISEntities();
            List<DeliveryOrder> deliverList = db.DeliveryOrders.ToList();
            return View(deliverList);
        }
    }
}