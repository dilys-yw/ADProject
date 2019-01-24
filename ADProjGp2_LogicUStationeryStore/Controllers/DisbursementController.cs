using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using ADProjGp2_LogicUStationeryStore.Models;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{    
    public class DisbursementController : Controller
    {        
        DisbursementBusinessLogic disburseBizLogic = new DisbursementBusinessLogic();
        [HttpPost]
        public async Task<ActionResult> GenerateDisbursement(List<RetrievalListModel> retrievalList)
        {
            await disburseBizLogic.DisbursementAllocation(retrievalList, Session);
            return RedirectToAction("DisbursementListSummary","Disbursement");
        }

        public ActionResult DisbursementListSummary()
        {
            List<DisbursementHistoryViewModel> disburseHVM = disburseBizLogic.GetDisbursementHistoryViewModels();
            disburseHVM.Sort(new DisbursementComparerDisDate());
            return View(disburseHVM);
        }

        public ActionResult DisbursementListDetail(string retrievalID)
        {
            List<DisbursementViewModel> dvmList = disburseBizLogic.GenerateDisbursementViewModels(retrievalID, Session);
            Session["dvmList"] = dvmList;
            return View(dvmList);
        }

        public ActionResult WelcomeDisburseIDRedirection(string disburseID, string retID)
        {
            List<DisbursementViewModel> dvmList = disburseBizLogic.GenerateDisbursementViewModels(retID, Session);
            DisbursementViewModel disburseVM = dvmList.Find(x => x.disbursementID == disburseID);
            ViewBag.EmpList = (Dictionary<string, string>)Session["EmployeeList"];
            return View("DisbursementDetail", disburseVM);
        }

        public ActionResult DisbursementDetail(string disburseID)
        {
            List<DisbursementViewModel> dvmList = (List < DisbursementViewModel > ) Session["dvmList"];
            DisbursementViewModel disburseVM = dvmList.Find(x => x.disbursementID == disburseID);
            ViewBag.EmpList = (Dictionary<string, string>)Session["EmployeeList"];
            return View(disburseVM);
        }

        public ActionResult DisbursementDetailForRep(string disburseID)
        {
            DisbursementViewModel dvmList = disburseBizLogic.GenerateSingleDisbursementViewModel(disburseID, Session);
            return View(dvmList);
        }

        public async Task<ActionResult> DisbursementUserConfirm(string disburseID)
        {
            await disburseBizLogic.UserConfirmDisbursement(disburseID);
            DisbursementViewModel dvmList = disburseBizLogic.GenerateSingleDisbursementViewModel(disburseID, Session);
            return RedirectToAction("DisbursementDetailForRep",new { disburseID = disburseID });
        }

        public ActionResult DisbursementListFOrRep()
        {
            if (Session == null)
            {
                RedirectToAction("Login", "Home");
            }
            SSISEntities db = new SSISEntities();
            string employeeID = Session["EmployeeID"].ToString();
            List<Disbursement> disburseList = disburseBizLogic.GetDisbursementListForUser(Session);
            return View(disburseList);
        }

        public ActionResult DisbursementAdjustments(DisbursementViewModel model)
        {
            AdjustmentBusinessLogic adjustmentBusinessLogic = new AdjustmentBusinessLogic();
            DisbursementViewModel newmodel = adjustmentBusinessLogic.CreateDisbursementAdjustmentVoucher(model,Session);
            return View("DisbursementDetail", newmodel);
        }
    }
}