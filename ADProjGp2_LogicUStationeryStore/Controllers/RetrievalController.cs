using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using System.Collections;
using System.Threading.Tasks;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class RetrievalController : Controller
    {
        RetrievalBusinessLogic retrievalBizLogic = new RetrievalBusinessLogic();

        public ActionResult RetrievalListPreparation()
        {
            ModelState.Clear();
            List<ReqListSummary> list = retrievalBizLogic.CreateReqListSummaries();
            return View(list);
        }

        [HttpPost]
        public ActionResult RetrievalListPreparation(List<ReqListSummary> viewModel)
        {
            List<ReqListSummary> newList = new List<ReqListSummary>();
            if (viewModel != null)
            {
                newList = viewModel.Where(x => x.IsSelected == true).ToList<ReqListSummary>();
                return View(newList);
            }            
            return RedirectToAction("RetrievalListPreparation");
        }

        [HttpPost]
        public ActionResult RetrievalPreSave(List<ReqListSummary> retList)
        {
            ArrayList aList = new ArrayList();
            List <RetrievalListModel> retrievalList = new List<RetrievalListModel>();
            retList = retList.Where(x => x.IsSelected == true).ToList<ReqListSummary>();
            aList = retrievalBizLogic.GetRetrievalListModelsList(retrievalBizLogic.GetRequistions(retList));
            if (aList != null)
            {
                TempData["aList"] = aList;
            }
            else
            {
                List<ReqListSummary> tempReqList = retList.Where(x => x.IsSelected == true).ToList();
                TempData["tempRetList"] = tempReqList;
                return RedirectToAction("InventoryInsufficient");
            }
            return RedirectToAction("RetrievalSave");
        }

        public ActionResult InventoryInsufficient()
        {
            List<ReqListSummary> tempReqList = (List<ReqListSummary>)TempData["tempRetList"];
            List<InsufficientStockModel> model = retrievalBizLogic.GenerateInsufficientStockModels(tempReqList);
            return View(model);
        }

        public ActionResult RetrievalSave(List<RetrievalListModel> retrievalList)
        {
            ArrayList aList = new ArrayList();            
            if(TempData["aList"] != null)
            {
                aList = (ArrayList)TempData["aList"];
                retrievalList = (List<RetrievalListModel>)aList[0];
                ViewBag.retSum = (List<RetrievalListSummaryModel>)aList[1];
                return View(retrievalList);
            }
            string clerkID = Session["EmployeeID"].ToString();
            retrievalBizLogic.SaveRetrievalList(retrievalList, clerkID);
            List<RetrievalListSummaryModel> retSumModel = retrievalBizLogic.RetrievalListSumModelGeneration(retrievalList);
            aList = retrievalBizLogic.RetrievalGenerationViewModel(retrievalList, retSumModel);
            retrievalList = (List<RetrievalListModel>)aList[0];
            ViewBag.retSum = (List<RetrievalListSummaryModel>)aList[1];
            return View(retrievalList);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmRetrievalList(List<RetrievalListModel> retrievalList)
        {
            string clerkID = Session["EmployeeID"].ToString();
            retrievalBizLogic.SaveRetrievalList(retrievalList, clerkID);
            bool haveWarning = retrievalBizLogic.CheckRetrievalSubmissionWarning(retrievalList);
            if(haveWarning)
            {
                List<RetrievalListSummaryModel> rsm = retrievalBizLogic.RetrievalListSumModelGeneration(retrievalList);
                ArrayList aList = retrievalBizLogic.RetrievalGenerationViewModel(retrievalList, rsm);
                TempData["aList"] = aList;
                return RedirectToAction("RetrievalSave");
            }
            retrievalBizLogic.ConfirmRetrievalList(retrievalList, clerkID);
            List<RetrievalListSummaryModel> retSumModel = retrievalBizLogic.RetrievalListSumModelGeneration(retrievalList);
            DisbursementBusinessLogic disburseBizLogic = new DisbursementBusinessLogic();
            await disburseBizLogic.DisbursementAllocation(retrievalList, Session);
            ViewBag.retSum = retSumModel;
            return View(retrievalList);
        }

        public ActionResult RetrievalHistory()
        {
            List<RetListSummary> history = retrievalBizLogic.GetRetrievalListHistory();
            history.Sort(new RetListComparerByCreateDate());
            return View(history);
        }

        public ActionResult RetrievalDetail(string retrievalListID)
        {
            ArrayList aList = new ArrayList();
            List<RetrievalListModel> retrievalList = new List<RetrievalListModel>();
            aList = retrievalBizLogic.GetRetrievalListDetail(retrievalListID);
            retrievalList = (List<RetrievalListModel>)aList[0];
            ViewBag.retSum = (List<RetrievalListSummaryModel>)aList[1];
            ViewBag.DisburseCheck = retrievalBizLogic.CheckRetrievalDetailDisbursementStatus(retrievalList);
            return View(retrievalList);
        }

        public ActionResult RetrievalPending(string retrievalListID)
        {
            ArrayList aList = new ArrayList();
            List<RetrievalListModel> retrievalList = new List<RetrievalListModel>();
            aList = retrievalBizLogic.GetRetrievalListDetail(retrievalListID);
            TempData["aList"] = aList;
            return RedirectToAction("RetrievalSave");
        }

        public ActionResult ReqIDsPopup()
        {
            List<RetListSummary> list = retrievalBizLogic.GetRetrievalListHistory();
            return View(list);
        }
    }
}