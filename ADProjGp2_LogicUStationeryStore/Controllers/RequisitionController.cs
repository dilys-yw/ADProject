using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class RequisitionController : Controller
    {        
        private RequisitionBusinessLogic reqBizLogic = new RequisitionBusinessLogic();
        private CommonBusinessLogic commonBusinessLogic = new CommonBusinessLogic();
        private static RequisitionFormItemCart itemCart = new RequisitionFormItemCart();

        // New Requistion Form creation
        public ActionResult ReqForm()
        {
            if (TempData["INSession"] == null)
            {
                itemCart.RemoveAllCartItem();
            }
            return View(itemCart);
        }

        // Postback for adding items
        [HttpPost]
        public ActionResult ReqForm(string addItem, string addQty)
        {
            int x = 0;
            if (int.TryParse(addQty, out x))
            {
                itemCart.ItemCartAddDecision(addItem, x);
            }
            return View(itemCart);
        }

        // Deleting item
        public ActionResult Delete(int id)
        {
            itemCart.RemoveCartItem(id);
            TempData["INSession"] = true;
            return RedirectToAction("ReqForm");
        }

        //Triggers the requisition form packaging
        public ActionResult Generate()
        {
            //return on empty cart
            if (itemCart.RequestItemCart().Count()<=0)
                return RedirectToAction("ReqForm");

            return RedirectToAction("ReqFormSubmit");
        }

        //Pseudo-state page for requisition form submission - the final confirm
        public ActionResult ReqFormSubmit()
        {
            Requisition requisition = reqBizLogic.RequisitionFormGeneration(Session, itemCart);
            TempData["RequisitionFormGeneration"] = requisition;
            return View(requisition);
        }

        //Submission Actual - saving into DB
        public async Task<ActionResult> ReqFormSubmission()
        {
            Requisition requisition = (Requisition)TempData["RequisitionFormGeneration"];
            requisition = await reqBizLogic.SaveRequisitionForm(requisition, Session);
            TempData["RequisitionFormGeneration"] = requisition;
            return RedirectToAction("ReqFormDetail");
        }

        //Email notification to dept head about new submission
        public ActionResult ReqFromSubmitEmailNotification()
        {
            //Head to receive email
            return View();
        }

        // List of requisition for viewing
        public ActionResult RequisitionHistory()
        {
            List<Requisition> myReqList = reqBizLogic.MyRequisitionHistoryList(Session);
            myReqList.Sort(new RequisitionComparerApprovalDate());
            return View(myReqList);
        }

        // See Requisition Details
        public ActionResult ReqFormDetail(string id)
        {
            Requisition requisition = new Requisition();
            if (id == "" || id is null)
                requisition = (Requisition)TempData["RequisitionFormGeneration"];
            try
            {
                SSISEntities db = new SSISEntities();
                requisition = db.Requisitions.Where(x => x.requisitionId == id).First();
                if (requisition.RequisitionDetails == null)
                {
                    requisition = reqBizLogic.GetRequisitionDetails(requisition);
                }
            }
            catch (Exception e)
            {
                //log
            }
            return View(requisition);
        }

        //Requisition Approval
        public async Task<ActionResult> ReqApprove(Requisition requisition)
        {
            requisition = await reqBizLogic.ApproveRequisitionForm(requisition, Session);
            TempData["RequisitionFormGeneration"] = requisition;
            return RedirectToAction("ReqFormDetail");
        }

        public ActionResult ReqFromApproveEmailNotification()
        {
            //User to receive email
            return View();
        }

        //Requisition Rejection
        public async Task<ActionResult> ReqReject(Requisition requisition)
        {
            requisition = await reqBizLogic.RejectRequisitionForm(requisition, Session);
            TempData["RequisitionFormGeneration"] = requisition;
            return RedirectToAction("ReqFormDetail");
        }

        public ActionResult ReqFromRejectEmailNotification()
        {
            //User to receive email
            return View();
        }
    }
}
