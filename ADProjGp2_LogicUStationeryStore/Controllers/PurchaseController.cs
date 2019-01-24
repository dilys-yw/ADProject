using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using System.Threading.Tasks;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class PurchaseController : Controller
    {
        PurchaseBusinessLogic purchaseBizLogic = new PurchaseBusinessLogic();

        public ActionResult NewPurchaseOrder()
        {
           List<SuggestionViewModel> listSuggest = purchaseBizLogic.GetSuggestionList();

            return View(listSuggest);
        }

        [HttpPost]
        public ActionResult NewPurchaseOrder(List<SuggestionViewModel> svm)
        {
            List<SuggestionViewModel> itemList = new List<SuggestionViewModel>();
            for (int i = 0; i < svm.Count; i++)
            {
                if (svm[i].IsSelected)
                {
                    itemList.Add(svm[i]);
                }
            }
            TempData["ItemList"] = itemList;
            return Redirect("PurchaseOrder");
        }
        
        public ActionResult PurchaseOrder()
        {
            List<SuggestionViewModel> list = (List< SuggestionViewModel > )TempData["itemList"];
            List<PurchaseOrderViewModel> itemList = purchaseBizLogic.GetPurchaseOrderItem(list);
          
            return View(itemList);           
        }
        
        [HttpPost]
        public ActionResult PurchaseOrder(List<PurchaseOrderViewModel> pvm)
        {
            TempData["Selected"] = pvm;

            return Redirect("ConfirmPurchaseOrder");
        }

        public ActionResult ConfirmPurchaseOrder()
        {
            List<PurchaseOrderViewModel> list = (List<PurchaseOrderViewModel>)TempData["Selected"];
            CustomPOViewModel cpvm = purchaseBizLogic.ConfirmPurchaseOrder(list);
            TempData["CustomPOViewModel"] = cpvm;
            return View(cpvm);
        }

        [HttpPost]
        public ActionResult ConfirmPurchaseOrder(CustomPOViewModel cpvm)
        {
            CustomPOViewModel cpo = (CustomPOViewModel) TempData["CustomPOViewModel"];
            purchaseBizLogic.AddPurchaseOrder(cpo,Session);

            return Redirect("PurchaseOrderList");
        }

        public ActionResult PurchaseOrderList()
        {
            List<PurchaseOrder> results = purchaseBizLogic.DisplayList();
            return View(results);
        }

        public ActionResult PurchaseOrderDetail(string id)
        {
            List<PurchaseOrderViewModel> pod = new List<PurchaseOrderViewModel>();
            ViewBag.ValuePairs = purchaseBizLogic.PreparePurchaseOrderDetails(Session, id , out pod);
            if (pod == null)
                return View("NotFound");
            else
                return View("PurchaseOrderDetail", pod);           
        }

     
    }
}