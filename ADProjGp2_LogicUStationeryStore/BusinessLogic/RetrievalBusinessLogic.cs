using ADProjGp2_LogicUStationeryStore.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class RetrievalBusinessLogic : Controller
    {
        private SSISEntities db = new SSISEntities();
        public List<ReqListSummary> CreateReqListSummaries()
        {
            var details = (from req in db.Requisitions
                           join dept in db.DeptCollectionDetails
                           on req.departmentId equals dept.departmentId
                           where (req.status == "Approved" || req.status == "Special")
                           select new ReqListSummary
                           {
                               reqID = req.requisitionId,
                               deptName = dept.departmentName,
                               empName = req.employee,
                               approveDate = req.approvalDate.ToString(),
                               IsSelected = true,
                           }).ToList();
            return details;
        }

        public List<Requisition> GetRequistions(List<ReqListSummary> retList)
        {
            List<Requisition> returnList = new List<Requisition>();
            foreach (ReqListSummary x in retList)
            {
                returnList.Add(db.Requisitions.Where(y => y.requisitionId == x.reqID).First());
            }
            return returnList;
        }

        // [Retrieval Generation ViewModel Creation] - Retrieval Generation ViewModel creation process
        public ArrayList GetRetrievalListModelsList(List<Requisition> reqList)
        {
            if (!reqList.Any())
            {
                return null;
            }
            // [Retrieval Generation ViewModel Creation Step 1]
            reqList = checkSpecialFulfillment(reqList);
            //special situation where reqs consist only of specials which cannot be fulfilled
            if (!reqList.Any())
            {
                return null;
            }
            // [Retrieval Generation ViewModel Creation Step 2]
            List<RetrievalListModel> retrievalListModels = RetrievalListModelGeneration(reqList);
            // Iterate over lines of Retrievals data to generate summary for RetrievalGeneration viewModel - Also summary data is used for auto-allocation purposes
            List<RetrievalListSummaryModel> retrievalListSummaryModels = RetrievalListSumModelGeneration(retrievalListModels);
            // pump both lists to obtain the finalised viewModel
            ArrayList arrayList = RetrievalGenerationViewModel(retrievalListModels, retrievalListSummaryModels);
            return arrayList;
        }

        // [Retrieval Generation ViewModel Creation Step 1] - By default, Specials will be automatically excluded in retrieval process IF the inventory CANNOT fulfill the special requisition (to avoid creating special on special) 
        // The method will remove all the "Special Requisition" that do not fulfill the conditions - (1) Special can be fulfilled by existing Stock Qty (inventory)
        public List<Requisition> checkSpecialFulfillment(List<Requisition> specialReqList)
        {
            Dictionary<string, int> itemTally = new Dictionary<string, int>();
            List<Inventory> inventories = db.Inventories.ToList<Inventory>();
            List<Requisition> originalReqList = specialReqList;
            List<Requisition> filteredList = specialReqList.Where(x => x.status == "Special").ToList<Requisition>();
            if(!filteredList.Any())
            {
                return specialReqList;
            }
            foreach (Requisition x in specialReqList)
            {
                if (x.status == "Special")
                {
                    foreach (RequisitionDetail y in x.RequisitionDetails)
                    {
                        //check key
                        if (!itemTally.ContainsKey(y.itemId))
                        {
                            Inventory tempInv = inventories.Where(a => a.itemId == y.itemId).First();
                            //cannot fulfill
                            if (tempInv.storeQuantity < y.requestQty)
                            {
                                x.remark = "Remove";
                            }
                            itemTally.Add(y.itemId, tempInv.storeQuantity - y.requestQty);
                        }
                        // Key present , edit qty
                        else
                        {
                            if (itemTally[y.itemId] < y.requestQty)
                            {
                                x.remark = "Remove";
                            }
                            itemTally[y.itemId] = itemTally[y.itemId] - y.requestQty;
                        }
                    }
                }
            }
            specialReqList.RemoveAll(z => z.remark == "Remove");
            return specialReqList;
        }
        
        // [Retrieval Generation ViewModel Creation Step 2]
        // The method will create the RetrievalListModel (which creates detail lines of Retrieval List items on the View)
        public List<RetrievalListModel> RetrievalListModelGeneration(List<Requisition> reqList)
        {
            if(!reqList.Any())
            {
                return null;
            }
            List<RetrievalListModel> retrievalListModels = new List<RetrievalListModel>();            
            foreach (Requisition req in reqList)
            {
                //revisits
                if (req.RequisitionDetails.Count == 0)
                {
                    foreach(RequisitionDetail prereqdet in db.RequisitionDetails)
                    {
                        if(prereqdet.requisitionId == req.requisitionId)
                        {
                            req.RequisitionDetails.Add(prereqdet);
                        }
                    }
                }
                foreach (RequisitionDetail reqdet in req.RequisitionDetails)
                {
                    RetrievalListModel tempRetModel = new RetrievalListModel();
                    tempRetModel.requisitionID = req.requisitionId;
                    tempRetModel.approvalDate = (DateTime)req.approvalDate;
                    tempRetModel.department = req.departmentId;
                    if(reqdet.remark != null)
                    {
                        //Specials are treated differently - And must always be fulfilled first
                        if (reqdet.remark == "Special")
                        {
                            tempRetModel.allocateQty = reqdet.requestQty;
                            tempRetModel.reqQty = reqdet.requestQty;
                            tempRetModel.adjQty = reqdet.requestQty;
                            tempRetModel.IsAdjusted = true;
                            tempRetModel.remark = "Special Requisition";
                        }
                        //Remark will contain "Adjusted" (due to Adjustment made to adjqty) once they are saved
                        else if (reqdet.remark.Contains("Adjusted"))
                        {
                            tempRetModel.allocateQty = (int)reqdet.retrieveQty;
                            tempRetModel.reqQty = reqdet.requestQty;
                            tempRetModel.adjQty = reqdet.adjustQty;
                            tempRetModel.IsAdjusted = true;
                            tempRetModel.remark = "Adjusted";
                        }
                        else if (reqdet.remark.Contains("Fulfilled"))
                        {
                            tempRetModel.allocateQty = (int)reqdet.retrieveQty;
                            tempRetModel.reqQty = reqdet.requestQty;
                            tempRetModel.adjQty = reqdet.adjustQty;
                            tempRetModel.IsAdjusted = true;
                            tempRetModel.remark = "Final";
                        }
                    }
                    // By design, remark is null if not the above two status. Disbursed/fulfilled requisitions do not come here
                    else
                    {
                        tempRetModel.IsAdjusted = false;
                        tempRetModel.allocateQty = reqdet.requestQty;
                        tempRetModel.reqQty = reqdet.requestQty;
                        tempRetModel.remark = null;
                    }
                    Inventory tempInv = db.Inventories.Where(x => x.itemId == reqdet.itemId).First();
                    tempRetModel.stockQty = tempInv.storeQuantity;
                    tempRetModel.itemID = reqdet.itemId;
                    tempRetModel.itemName = db.Catalogues.Where(x => x.itemId == reqdet.itemId).First().description;
                    tempRetModel.itemBin = db.Catalogues.Where(x => x.itemId == reqdet.itemId).First().bin;                    
                    retrievalListModels.Add(tempRetModel);
                }
            }
            return retrievalListModels;
        }

        // [Retrieval Generation ViewModel Creation Step 3]
        // The method will create the RetrievalListSumModel (which creates summarised and collated Retrieval List items on the View - 2nd tab, which is easier for store clerk to retrieve from store)
        public List<RetrievalListSummaryModel> RetrievalListSumModelGeneration(List<RetrievalListModel> retrievalListModels)
        {
            if (!retrievalListModels.Any())
            {
                //No elements - exit
                return null;
            }
            List<RetrievalListSummaryModel> retrievalListSummaryModels = new List<RetrievalListSummaryModel>();
            foreach (RetrievalListModel ret in retrievalListModels)
            {
                RetrievalListSummaryModel tempRetSumModel = new RetrievalListSummaryModel();
                tempRetSumModel.requisitionID = new List<string>();
                if (retrievalListSummaryModels.Exists(x => x.itemID == ret.itemID && x.department == ret.department))
                {
                    tempRetSumModel = retrievalListSummaryModels.Find(x => x.itemID== ret.itemID && x.department == ret.department);
                    tempRetSumModel.retrieveQty += ret.reqQty;
                }
                else
                {
                    tempRetSumModel.department = ret.department;
                    tempRetSumModel.itemName = ret.itemName;
                    tempRetSumModel.itemID = ret.itemID;
                    tempRetSumModel.requisitionID.Add(ret.requisitionID);
                    tempRetSumModel.itemBin = ret.itemBin;
                    tempRetSumModel.retrieveQty = ret.reqQty;
                    tempRetSumModel.stockQty = ret.stockQty;
                    retrievalListSummaryModels.Add(tempRetSumModel);
                }
            }
            return retrievalListSummaryModels;
        }

        // [Retrieval Generation ViewModel Creation Step 4]
        // The method will create the entire ViewModel - after the necessary ALLOCATION logic
        public ArrayList RetrievalGenerationViewModel (List<RetrievalListModel> retrievalListModels, List<RetrievalListSummaryModel> retrievalListSummaryModels)
        {
            if (!retrievalListModels.Any() || !retrievalListSummaryModels.Any())
            {
                return null;
            }
            //Ensure total retrieval  <= stock for ALL items, and initialised the Total Allocation Amount (AllocateQty) for all items
            foreach (RetrievalListSummaryModel retSum in retrievalListSummaryModels)
            {
                // Cannot retrieve more than stock
                if (retSum.stockQty < retSum.retrieveQty)
                {
                    retSum.retrieveQty = retSum.stockQty;
                }
                // Cannot allocate more than stock
                retSum.allocateQty = retSum.stockQty;
                foreach (RetrievalListModel ret in retrievalListModels)
                {
                    // Allocate user adjustments first
                    if (retSum.itemID == ret.itemID && ret.adjQty != null)
                    {
                        retSum.allocateQty = retSum.allocateQty - (int)ret.adjQty;
                    }
                }
                if (retSum.allocateQty < 0)
                {
                    retSum.tallyQty = retSum.stockQty + Math.Abs(retSum.allocateQty);
                }
                else
                {
                    retSum.tallyQty = retSum.stockQty - retSum.allocateQty;
                }
            }

            //Sort the qty base on Approval Date - the 2nd allocation auto-logic of stock the earlier approved requistion(base on approved date) of the same item is being fulfilled first
            retrievalListModels.Sort(new RetrievalListComparerByAppDate());

            //Allocation starts
            foreach (RetrievalListSummaryModel retSum in retrievalListSummaryModels)
            {
                foreach (RetrievalListModel x in retrievalListModels)
                {
                    //No action for Finals
                    if (x.remark=="Final")
                    {
                        continue;
                    }
                    //Adjusted and Specials will not participate in the auto-allocation because the former is user decision and the latter is a design decision
                    else if (x.IsAdjusted || x.adjQty != null)
                    {
                        x.allocateQty = x.adjQty == null? 0: (int)x.adjQty;
                        continue;
                    }
                    else
                    {
                        if (x.itemID == retSum.itemID)
                        {
                            if (x.reqQty > retSum.allocateQty)
                            {
                                x.allocateQty = retSum.allocateQty;
                                retSum.allocateQty = 0;                               
                            }
                            else
                            {
                                x.allocateQty = x.reqQty;
                                retSum.allocateQty = retSum.allocateQty - x.allocateQty;
                            }
                        }
                    }
                }
            }
            //After allocation, sort the list long list according to item name
            retrievalListModels.Sort(new RetrievalListComparerByItemName());
            retrievalListSummaryModels.Sort(new RetrievalListSumComparerByItemName());
            ArrayList arrayList = new ArrayList();
            arrayList.Add(retrievalListModels);
            arrayList.Add(retrievalListSummaryModels);
            return arrayList;
        }        

        public void SaveRetrievalList(List<RetrievalListModel> retrievalListModels, string clerkID)
        {
            DateTime dateTime = DateTime.Now;
            Dictionary<string, string> itemChecker = checkInventoryQty(retrievalListModels);
            Requisition retCheckReq = db.Requisitions.Find(retrievalListModels.First().requisitionID);
            string originalRet = retCheckReq != null ? retCheckReq.retrievalId : null;
            
            Retrieval retrieval = new Retrieval();
            if (originalRet ==null)
            {
                retrieval.clerkId = clerkID;
                retrieval.retrievalCreationDate = dateTime;
                retrieval.retrievalId = "RET-" + clerkID + "-" + dateTime.ToString("yyMMddhhmmssffff");
                retrieval.status = "Pending";
                db.Retrievals.Add(retrieval);
            }
            //Find reqs for each line and change status + remarks + assign ID
            foreach (RetrievalListModel x in retrievalListModels)
            {
                Requisition requisition = db.Requisitions.Find(x.requisitionID);
                requisition.retrievalId = originalRet == null ? retrieval.retrievalId : originalRet;
                requisition.status = "Retrieval Pending";

                foreach (RequisitionDetail y in requisition.RequisitionDetails)
                {
                    if (y.remark != null)
                    {
                        if (!y.remark.Contains("Special"))
                        {
                            //itemID is unique in EACH Requistion, break once found to save iteration time
                            if (y.itemId == x.itemID)
                            {
                                y.retrieveQty = x.allocateQty;
                                //only do something if User amends qty
                                if (x.adjQty != null || x.adjQty == 0)
                                {
                                    if (x.adjQty < 0)
                                    {
                                        y.remark = "Warning - Please do not enter negative numbers";
                                        continue;
                                    }
                                    y.adjustQty = x.adjQty;
                                    y.retrieveQty = x.adjQty;
                                    y.remark = "Adjusted";
                                    x.remark = itemChecker[y.itemId] == "" ? "Adjusted" : "Adjusted" + itemChecker[y.itemId];
                                }
                                else
                                {
                                    y.adjustQty = null;
                                    y.remark = null;
                                    x.remark = null;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (y.itemId == x.itemID)
                        {
                            y.retrieveQty = x.allocateQty;
                            //only do something if User amends qty
                            if (x.adjQty != null || x.adjQty == 0)
                            {
                                if (x.adjQty < 0)
                                {
                                    y.remark = "Warning - Please do not enter negative numbers";
                                    continue;
                                }
                                y.adjustQty = x.adjQty;
                                y.retrieveQty = x.adjQty;
                                y.remark = "Adjusted";
                                x.remark = itemChecker[y.itemId] == "" ? "Adjusted" : "Adjusted" + itemChecker[y.itemId];
                            }
                            else
                            {
                                y.adjustQty = null;
                                y.remark = null;
                                x.remark = null;
                            }
                            break;
                        }
                    }
                }
            }
            db.SaveChanges();
        }

        public Dictionary<string,string> checkInventoryQty(List<RetrievalListModel> retrievalList)
        {
            Dictionary<string, int> itemIDs = new Dictionary<string, int>();
            //built qty Dictionary
            foreach(RetrievalListModel rlm in retrievalList)
            {
                if(itemIDs.ContainsKey(rlm.itemID))
                {
                    itemIDs[rlm.itemID] = itemIDs[rlm.itemID] + Math.Max(rlm.allocateQty, rlm.adjQty==null ? 0 : (int)rlm.adjQty);
                }
                else
                {
                    itemIDs.Add(rlm.itemID, Math.Max(rlm.allocateQty, rlm.adjQty == null ? 0 : (int)rlm.adjQty));
                }
            }
            //built Comments Dictionary
            Dictionary<string, string> itemComments = new Dictionary<string, string>();
            foreach(KeyValuePair<string,int> kvm in itemIDs)
            {
                int currentstoreQty = db.Inventories.Find(kvm.Key).storeQuantity;
                if (kvm.Value > currentstoreQty)
                {
                    string kvmstring = "Warning - Total quantity retrieved (" + kvm.Value + " is more than store quantity (" + currentstoreQty + ")" + " - Retrieval cannot be confirmed";
                    itemComments.Add(kvm.Key, kvmstring);
                }
                else if (kvm.Value < 0)
                {
                    string kvmstring = "Warning - Negative numbers not allowed";
                    itemComments.Add(kvm.Key, kvmstring);
                }
                else
                {
                    string kvmstring = "";
                    itemComments.Add(kvm.Key, kvmstring);
                }
            }
            return itemComments;
        }

        public bool CheckRetrievalSubmissionWarning(List<RetrievalListModel> retrievalListModels)
        {
            bool HaveWarning = false;
            foreach (RetrievalListModel x in retrievalListModels)
            {
                if (x.remark != null)
                {
                    if (x.remark.Contains("Warning"))
                    {
                        HaveWarning = true;
                    }
                }
            }
            return HaveWarning;
        }

        public void ConfirmRetrievalList(List<RetrievalListModel> retrievalListModels, string clerkID)
        {
            DateTime dateTime = DateTime.Now;
            Requisition retCheckReq = db.Requisitions.Find(retrievalListModels.First().requisitionID);
            string originalRet = retCheckReq != null ? retCheckReq.retrievalId : null;

            Retrieval retrieval = new Retrieval();
            if (originalRet == null)
            {
                //Create retrieval and edit params accordingly;
                retrieval.clerkId = clerkID;
                retrieval.retrievalCreationDate = dateTime;
                retrieval.retrievalId = "RET-" + clerkID + "-" + dateTime.ToString("yyMMddhhmmssffff");
                retrieval.status = "Confirmed";
                db.Retrievals.Add(retrieval);
            }
            else
            {
                //change retrieval status for existing retrieval;
                RetrievalListModel rlm = retrievalListModels.Where(x => !x.requisitionID.Contains("Special")).FirstOrDefault();
                string reqID = rlm == null ? null : rlm.requisitionID;
                string retID = reqID == null ? null : db.Requisitions.Find(reqID).retrievalId;
                if (retID != null)
                {
                    retrieval = db.Retrievals.Find(retID);
                    retrieval.status = "Confirmed";
                }
            }
            //Find reqs for each line and change status + remarks + assign ID
            foreach (RetrievalListModel x in retrievalListModels)
            {
                Requisition requisition = db.Requisitions.Find(x.requisitionID);

                //creates retrievalID in req
                if (requisition.status == "Retrieval Pending")
                {
                    requisition.retrievalId = retrieval.retrievalId;
                    requisition.status = "Retrieval Confirmed";
                    requisition.remark = requisition.remark + " , Retrieval confirmed on" + dateTime;
                }
                else
                {
                    requisition.retrievalId = retrieval.retrievalId;
                    requisition.status = "Retrieval Confirmed";
                    requisition.remark ="Retrieval confirmed on" + dateTime;
                }

                foreach (RequisitionDetail y in requisition.RequisitionDetails)
                {
                    if (y.remark != null)
                    {
                        if (y.remark.Contains("Special"))
                        {
                            //itemID is unique in EACH Requistion, break once found to save iteration time
                            if (y.itemId == x.itemID)
                            {
                                y.retrieveQty = x.allocateQty;
                                //only do something if User amends qty
                                if (x.adjQty != null || x.adjQty == 0)
                                {
                                    y.adjustQty = x.adjQty;
                                    y.retrieveQty = x.adjQty;
                                    y.remark = "Adjusted";
                                }
                                else
                                {
                                    y.adjustQty = null;
                                    y.remark = null;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        //itemID is unique in EACH Requistion, break once found to save iteration time
                        if (y.itemId == x.itemID)
                        {
                            y.retrieveQty = x.allocateQty;
                            //only do something if User amends qty
                            if (x.adjQty != null || x.adjQty == 0)
                            {
                                y.adjustQty = x.adjQty;
                                y.retrieveQty = x.adjQty;
                                y.remark = "Adjusted";
                            }
                            else
                            {
                                y.adjustQty = null;
                                y.remark = null;
                            }
                            break;
                        }
                    }
                }
            }

            db.SaveChanges();
        }

        public List<InsufficientStockModel> GenerateInsufficientStockModels (List<ReqListSummary> reqList)
        {
            List<InsufficientStockModel> model = new List<InsufficientStockModel>();
            foreach (ReqListSummary x in reqList)
            {
                foreach(RequisitionDetail reqdet in db.RequisitionDetails)
                {
                    if (reqdet.requisitionId == x.reqID)
                    {
                        InsufficientStockModel ism = new InsufficientStockModel();
                        ism.requisitionID = reqdet.requisitionId;
                        ism.requestQty = reqdet.requestQty;
                        ism.itemID = reqdet.itemId;
                        ism.itemName = db.Catalogues.Find(reqdet.itemId).description;
                        ism.stockQty = db.Inventories.Find(reqdet.itemId).storeQuantity;
                        model.Add(ism);
                    }
                }
            }
            return model;
        }

        public bool CheckRetrievalDetailDisbursementStatus(List<RetrievalListModel> retrievalListModels)
        {
            bool returnBool = false;
            foreach(RetrievalListModel x in retrievalListModels)
            {
                Requisition req = db.Requisitions.Find(x.requisitionID);
                returnBool = req.disbursementId == null ? false : true;
                if (returnBool == true)
                {
                    return true;
                }
            }
            return false;
        }

        public ArrayList GetRetrievalListDetail(string retrievalListID)
        {
            ArrayList arrayList = new ArrayList();
            List<Requisition> reqList = db.Requisitions.Where(x => x.retrievalId == retrievalListID).ToList();
            List<RetrievalListModel> retrievalListModels = RetrievalListModelGeneration(reqList);
            List<RetrievalListSummaryModel> retrievalListSummaryModels = RetrievalListSumModelGeneration(retrievalListModels);
            arrayList = RetrievalGenerationViewModel(retrievalListModels, retrievalListSummaryModels);
            return arrayList;
        }

        public List<RetListSummary> GetRetrievalListHistory()
        {
            List<RetListSummary> retListSummaries = new List<RetListSummary>();
            List<Retrieval> retrievals = db.Retrievals.ToList<Retrieval>();
            foreach(Retrieval x in retrievals)
            {
                RetListSummary retListSummary = new RetListSummary();
                retListSummary.retID = x.retrievalId;
                retListSummary.clerkName = x.clerkId;
                retListSummary.creationDate = x.retrievalCreationDate.ToShortDateString();
                retListSummary.status = x.status;
                retListSummary.reqIDs = db.Requisitions.Where(y => y.retrievalId == x.retrievalId).Select(z => z.requisitionId).ToList<string>();
                retListSummaries.Add(retListSummary);
            }
            return retListSummaries;
        }
    }
}