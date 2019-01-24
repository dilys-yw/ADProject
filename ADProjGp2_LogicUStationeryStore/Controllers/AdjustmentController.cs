using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class AdjustmentController : Controller
    {
        AdjustmentBusinessLogic adjBizLogic = new AdjustmentBusinessLogic();
        private static AdjustmentVoucherViewModel avvm;
        private static List<AdjustmentVoucherViewModelDetail> tempList;

        public ActionResult AdjustmentVoucherCreation()
        {
            avvm = adjBizLogic.InitiateAdjustmentVoucher(Session);
            tempList = new List<AdjustmentVoucherViewModelDetail>();
            avvm.itemList = tempList;        
            return View(avvm);
        }

        [HttpPost]
        public ActionResult AdjustmentVoucherCreation(string adjItem, string adjQty)
        {
            if (int.TryParse(adjQty, out int adjQtyTP))
            {
                adjBizLogic.AddItem(avvm, adjItem, adjQtyTP);
            }
            return View(avvm);
        }

        [HttpPost]
        public ActionResult ItemDelete(string adjItem)
        {
            adjBizLogic.DeleteItem(avvm, adjItem);
            return View("AdjustmentVoucherCreation", avvm);
        }

        [HttpPost]
        public ActionResult AdjustmentVoucherSave(AdjustmentVoucherViewModel avvmnew)
        {
            avvm = avvmnew;
            return View("AdjustmentVoucherCreation",avvm);
        }

        [HttpPost]
        public ActionResult AdjustmentVoucherConfirm(AdjustmentVoucherViewModel avvmnew)
        {
            adjBizLogic.CreateAdjustmentVoucher(avvmnew);
            return RedirectToAction("AdjustmentDetail", new { adjustmentID = avvm.voucherID });
        }

        public ActionResult AdjustmentDetail(string adjustmentID)
        {
            AdjustmentVoucherViewModel avvm = adjBizLogic.GetAdjustmentVoucher(adjustmentID, Session);
            return View(avvm);
        }
        
        public ActionResult AdjustmentHistory()
        {
            List<AdjustmentVoucherViewModel> avvmList = adjBizLogic.GetAdjustmentHistory(Session);
            avvmList.Sort(new AdjustmentComparerVoucherDate());
            return View(avvmList);
        }

        public ActionResult AdjustmentVoucherApprove(AdjustmentVoucherViewModel avvmnew)
        {
            avvm = adjBizLogic.ApproveVoucher(avvmnew);
            return View("AdjustmentVoucherCreation", avvm);
        }

    }
}