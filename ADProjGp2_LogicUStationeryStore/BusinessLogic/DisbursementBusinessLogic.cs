using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{    
    public class DisbursementBusinessLogic : Controller
    {
        private SSISEntities db = new SSISEntities();
        public async Task DisbursementAllocation(List<RetrievalListModel> validatedRetrievalListModels, HttpSessionStateBase session)
        {
            validatedRetrievalListModels.Sort(new RetrievalListComparerBySpecial());
            Dictionary<string,string> departmentIDs = new Dictionary<string,string>();
            string deptID = "";
            int specCounter = 0;
            //Settle all Specials
            foreach (RetrievalListModel x in validatedRetrievalListModels)
            {
                specCounter++;
                //Disbursement is handled via departments
                Disbursement tempDisbursement = new Disbursement();
                DateTime time = DateTime.Now;
                string timeStamp = time.ToString("yyyyMMddhhmmssffff");                
                string disburseID = "";
                //only create disbursement if there is an allocation (no allocation no disbursement)
                if (x.allocateQty != 0)
                {
                    if (departmentIDs.ContainsKey(x.department))
                    {
                        disburseID = departmentIDs[x.department];
                        tempDisbursement.comment = tempDisbursement.comment == null ? "-" + x.requisitionID : tempDisbursement.comment  + "-" + x.requisitionID;
                    }
                    else
                    {
                        tempDisbursement.disbursementId = "DIS-" + x.department + "-" + timeStamp;
                        tempDisbursement.comment = x.requisitionID;
                        tempDisbursement.disburseDate = time;
                        tempDisbursement.status = "Awaiting Collection";
                        string repid = db.DeptCollectionDetails.Find(x.department).representative;
                        tempDisbursement.repID = repid;
                        db.Disbursements.Add(tempDisbursement);
                        departmentIDs.Add(x.department, tempDisbursement.disbursementId);
                        disburseID = tempDisbursement.disbursementId;                        
                    }
                }

                if (x.requisitionID.Contains("Special"))
                {
                    SpecialRequest tempSpecial = new SpecialRequest();
                    Requisition tempRequisition = new Requisition();
                    RequisitionDetail tempRequisitionDet = new RequisitionDetail();

                    //Fulfilling Special - Special will be removed from the retrieval processes
                    // note Reqid to be lookuped since reqid is specialID for specials
                    string reqID = db.SpecialRequests.Where(z => z.specialId == x.requisitionID).First().requisitionId;
                    tempRequisition = db.Requisitions.Find(reqID);
                    tempSpecial = db.SpecialRequests.Where(y => y.specialId == x.requisitionID && y.itemId == x.itemID).First();                    
                    tempSpecial.status = "Disbursed";

                    //trace back original requisition id to clear special flag
                    tempRequisitionDet = db.RequisitionDetails.Where(z => z.requisitionId == reqID && z.itemId == x.itemID).First();
                    tempRequisitionDet.remark = tempRequisitionDet.remark  == null ? "Fulfilled Qty:" + x.adjQty + " by " + disburseID : tempRequisitionDet.remark + " , Fulfilled Qty:" + x.adjQty + " by " + disburseID;

                    //trace back special requisition to edit status and remark
                    Requisition specialReq = db.Requisitions.Find("Special-" + reqID);
                    specialReq.disbursementId = disburseID;
                    specialReq.remark = specialReq.remark == null ? "Fulfilled by " + disburseID : specialReq.remark + " , Fulfilled by " + disburseID;
                    specialReq.status = "Full Disburse";
                    specialReq.RequisitionDetails.First().remark = "Fulfilled Qty:" + x.adjQty + " by " + disburseID;

                    //clear requisition if all reqdets are fulfiled
                    if (tempRequisition.RequisitionDetails.Select(z => z.remark.Contains("Fulfilled")).Count() == tempRequisition.RequisitionDetails.Count)
                    {
                        tempRequisition.status = "Full Disburse";
                        tempRequisition.remark = tempRequisition.remark == null ? "Fully Fulfilled by " + disburseID : tempRequisition.remark + " , Fully Fulfilled by " + disburseID;                        
                    }
                    else
                    {
                        tempRequisition.status = "Partial Disburse";
                        tempRequisition.remark = tempRequisition.remark == null ? "Partially Fulfilled by " + disburseID : tempRequisition.remark + " , Fully Fulfilled by " + disburseID;
                    }

                    //Moving stock from inventory to disbursement
                    Inventory invItem = db.Inventories.Where(y => y.itemId == x.itemID).First();
                    if (x.adjQty != null)
                    {
                        invItem.storeQuantity = invItem.storeQuantity - (int)x.adjQty;
                        invItem.disburseQuantity = invItem.disburseQuantity + (int)x.adjQty;
                    }
                    else
                    {
                        invItem.storeQuantity = invItem.storeQuantity - x.allocateQty;
                        invItem.disburseQuantity = invItem.disburseQuantity + x.allocateQty;
                    }
                }
                else
                {
                    SpecialRequest tempSpecial = new SpecialRequest();
                    Requisition tempRequisition = new Requisition();
                    RequisitionDetail tempRequisitionDet = new RequisitionDetail();

                    //Not special => retrieveQty can be lower than request
                    int delta = 0;
                    tempRequisitionDet = db.RequisitionDetails.Where(z => z.requisitionId == x.requisitionID && z.itemId == x.itemID).First();
                    tempRequisition = db.Requisitions.Where(z => z.requisitionId == x.requisitionID).First();
                    //full fulfillment
                    if (tempRequisitionDet.retrieveQty >= tempRequisitionDet.requestQty)
                    {
                        if (tempRequisitionDet.retrieveQty == tempRequisitionDet.requestQty)
                        {
                            tempRequisitionDet.remark = tempRequisitionDet.remark == null ? "Fulfilled normally" : tempRequisitionDet.remark + " , Fulfilled normally";
                        }
                        else
                        {
                            tempRequisitionDet.remark = tempRequisitionDet.remark == null ? "Over - Fulfilled by" + (tempRequisitionDet.retrieveQty - tempRequisitionDet.requestQty) :
                               tempRequisitionDet.remark + " , Over - Fulfilled by" + (tempRequisitionDet.retrieveQty - tempRequisitionDet.requestQty);
                        }
                    }
                    //adjustment result in partial / zero fulfillment
                    else if (tempRequisitionDet.retrieveQty < tempRequisitionDet.requestQty)
                    {
                        //finds the delta - which goes to the special creation
                        delta = tempRequisitionDet.requestQty - (int)tempRequisitionDet.retrieveQty;
                       
                        //create special for shortfall
                        tempSpecial.itemId = x.itemID;
                        tempSpecial.requestQty = delta;
                        tempSpecial.requisitionId = x.requisitionID;
                        tempSpecial.status = "Special";
                        tempSpecial.specialId = "Special-" + x.requisitionID + specCounter;
                        tempRequisitionDet.remark = tempRequisitionDet.remark == null ? " Special created (ID:" + tempSpecial.specialId + ") for qty:" + delta : tempRequisitionDet.remark + " Special created (ID:" + tempSpecial.specialId + ") for qty:" + delta;
                        db.SpecialRequests.Add(tempSpecial);

                        //create special requisitions after settling the specials
                        Requisition specialRequisition = new Requisition();
                        specialRequisition.approvalDate = tempRequisition.approvalDate;
                        specialRequisition.approvalPerson = tempRequisition.approvalPerson;
                        specialRequisition.departmentId = tempRequisition.departmentId;
                        specialRequisition.employee = tempRequisition.employee;
                        specialRequisition.remark = "Special created for " + tempRequisition.requisitionId + " Item : " + tempRequisitionDet.itemId + " Amount : " + delta;
                        specialRequisition.requisitionId = "Special-" + tempRequisition.requisitionId + "-" + tempRequisitionDet.itemId;
                        specialRequisition.requestDate = tempRequisition.requestDate;
                        specialRequisition.status = "Special";
                        specialRequisition.RequisitionDetails = new List<RequisitionDetail>();
                        RequisitionDetail specialReqDet = new RequisitionDetail();
                        specialReqDet.adjustQty = delta;
                        specialReqDet.requestQty = delta;
                        specialReqDet.adjustQty = delta;
                        specialReqDet.itemId = tempRequisitionDet.itemId;
                        specialReqDet.requisitionId = specialRequisition.requisitionId;
                        specialReqDet.remark = "Special";
                        specialRequisition.RequisitionDetails.Add(specialReqDet);
                        db.Requisitions.Add(specialRequisition);                        
                    }
                    //adjust inventory as long as not zero retrieval
                    if (tempRequisitionDet.retrieveQty != 0)
                    {
                        Inventory invItem = db.Inventories.Where(y => y.itemId == x.itemID).First();
                        invItem.storeQuantity = invItem.storeQuantity - (int)x.allocateQty;
                        invItem.disburseQuantity = invItem.disburseQuantity + (int)x.allocateQty;
                        //disbursement will also happen
                        tempRequisition.disbursementId = disburseID;
                    }
                }
            }
            await db.SaveChangesAsync();
            //adjust status of Requisition status after adjustments to reqdetail
            foreach (KeyValuePair<string, string> kvp in departmentIDs)
            {
                List<Requisition> reqlist = db.Requisitions.Where(x => x.disbursementId == kvp.Value).ToList();
                if(reqlist.Any())
                {
                    foreach(Requisition req in reqlist)
                    {
                        if(req.status !="Full Disburse")
                        {
                            if(req.RequisitionDetails.Where(y=>y.remark.Contains("Fulfilled")).ToList<RequisitionDetail>().Count == req.RequisitionDetails.Count)
                            {
                                req.status = "Full Disburse";
                                req.remark = req.remark + " , Fully Fulfilled by " + kvp.Value;
                            }
                            else
                            {
                                req.status = "Partial Disburse";
                                req.remark = req.remark + " , Partially Fulfilled by " + kvp.Value;
                            }
                        }
                    }
                }
            }
            await db.SaveChangesAsync();

            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            foreach (KeyValuePair<string, string> kvp in departmentIDs)
            {
                deptID = kvp.Key;
                string linkref = "http://" + ConstantsConfig.linkrefURLPartial + "/Disbursement/DisbursementDetailForRep?disburseID=" + kvp.Value;
                emailBizLogic.SendEmail("collectDisburse", null, null, deptID, null, linkref);            }

            await Task.CompletedTask;
        }



        public List<DisbursementHistoryViewModel> GetDisbursementHistoryViewModels()
        {
            List<string> retrievalIDs = new List<string>();
            retrievalIDs = db.Retrievals.Select(x => x.retrievalId).ToList();
            if (!retrievalIDs.Any())
            {
                return null;
            }
            List<DisbursementHistoryViewModel> dhvmList = new List<DisbursementHistoryViewModel>();
            foreach(string retID in retrievalIDs)
            {
                DisbursementHistoryViewModel dhvm = new DisbursementHistoryViewModel();
                dhvm.retrievalID = retID;
                dhvm.disbursementIDs = new List<string>();
                List<string> reqListWithRetID = db.Requisitions.Where(x => x.retrievalId == retID).Select(x => x.disbursementId).Distinct().ToList();
                foreach(string disburseID in reqListWithRetID)
                {
                    dhvm.disbursementIDs.Add(disburseID);
                }
                if (dhvm.disbursementIDs.Any())
                {
                    List<DateTime> dateTimes = new List<DateTime>();
                    foreach(string disburseid in dhvm.disbursementIDs)
                    {
                        if (disburseid != null)
                        {
                            DateTime? x = db.Disbursements.Find(disburseid).disburseDate;
                            if (x != null)
                                dateTimes.Add((DateTime)x);
                        }
                    }
                    if (dateTimes.Any())
                    {
                        dhvm.disburseDate = dateTimes.Max();
                        dhvmList.Add(dhvm);
                    }
                }
            }
            return dhvmList;
        }

        public List<DisbursementViewModel> GenerateDisbursementViewModels(string retrievalID, HttpSessionStateBase session)
        {
            List<DisbursementViewModel> disburseVMList = new List<DisbursementViewModel>();
            List<Requisition> requisitions = db.Requisitions.Where(x => x.retrievalId == retrievalID).ToList();
            Dictionary<string, string> empList = (Dictionary<string, string>)session["EmployeeList"];

            foreach (Requisition req in requisitions)
            {
                DisbursementViewModel dvm = new DisbursementViewModel();
                dvm.dvmdList = new List<DisbursementViewModelDetail>();
                //Initialise dvm if not created
                if (!disburseVMList.Where(x=>x.disbursementID== req.disbursementId).Any())
                {
                    dvm.disbursementID = req.disbursementId;
                    dvm.requestorDeptID = req.departmentId;
                    dvm.status = db.Disbursements.Where(x => x.disbursementId == req.disbursementId).First().status;
                    dvm.repName = db.DeptCollectionDetails.Where(x => x.departmentId == req.departmentId).First().representative;
                    dvm.collectionPoint = db.DeptCollectionDetails.Where(x => x.departmentId == req.departmentId).First().collectionPoint;
                    dvm.collectionTime = db.CollectionPoints.Where(x => x.locationName == dvm.collectionPoint).First().collectTime;
                    dvm.disburseDate = db.Disbursements.Where(x => x.disbursementId == req.disbursementId).First().disburseDate;
                    dvm.adjustmentID = db.Disbursements.Where(x => x.disbursementId == req.disbursementId).First().adjustmentID;
                    disburseVMList.Add(dvm);
                }
                else
                {
                    dvm = disburseVMList.Find(x => x.disbursementID == req.disbursementId);
                }
                //pack reqdet into dvmd
                foreach(RequisitionDetail reqdet in req.RequisitionDetails)
                {
                    DisbursementViewModelDetail dvmd = new DisbursementViewModelDetail();
                    dvmd.requestorID = req.employee;
                    dvmd.requestorName = empList[req.employee].ToString();
                    dvmd.requisitionID = req.requisitionId;
                    dvmd.itemID = reqdet.itemId;
                    dvmd.itemDescription = db.Catalogues.Find(reqdet.itemId).description;
                    dvmd.retrievalID = req.retrievalId;
                    dvmd.requestQty = reqdet.requestQty;
                    dvmd.retrieveQty = reqdet.retrieveQty;

                    AdjustmentDetail adjustmentDetail = db.AdjustmentDetails.Where(w => w.voucherId == dvm.adjustmentID && w.itemId == dvmd.itemID).FirstOrDefault();
                    if (adjustmentDetail != null)
                    {
                        dvmd.adjQty = adjustmentDetail.quantity;
                    }

                    dvm.dvmdList.Add(dvmd);
                }
            }
            return disburseVMList;
        }

        public DisbursementViewModel GenerateSingleDisbursementViewModel(string disburseID, HttpSessionStateBase session)
        {
            Dictionary<string, string> empList = (Dictionary<string, string>)session["EmployeeList"];
            Requisition req = db.Requisitions.Where(x => x.disbursementId == disburseID).First();
            DisbursementViewModel dvm = new DisbursementViewModel();
            dvm.dvmdList = new List<DisbursementViewModelDetail>();
            //Initialise dvm
            dvm.disbursementID = req.disbursementId;
            dvm.requestorDeptID = req.departmentId;
            dvm.deptID = req.departmentId;
            dvm.repName = session["EmployeeID"].ToString();
            dvm.requestorName = req.employee;
            dvm.status= db.Disbursements.Where(x => x.disbursementId == req.disbursementId).First().status;
            dvm.collectionPoint = db.DeptCollectionDetails.Where(x => x.departmentId == req.departmentId).First().collectionPoint;
            dvm.collectionTime = db.CollectionPoints.Where(x => x.locationName == dvm.collectionPoint).First().collectTime;
            dvm.disburseDate = db.Disbursements.Where(x => x.disbursementId == req.disbursementId).First().disburseDate;

            //pack reqdet into dvmd
            foreach (RequisitionDetail reqdet in req.RequisitionDetails)
            {
                DisbursementViewModelDetail dvmd = new DisbursementViewModelDetail();
                dvmd.requestorID = req.employee;
                dvmd.requestorName = empList[req.employee].ToString();
                dvmd.requisitionID = req.requisitionId;
                dvmd.itemID = reqdet.itemId;
                dvmd.itemDescription = db.Catalogues.Find(reqdet.itemId).description;
                dvmd.retrievalID = req.retrievalId;
                dvmd.requestQty = reqdet.requestQty;
                dvmd.retrieveQty = reqdet.retrieveQty;
                dvm.dvmdList.Add(dvmd);
            }
            return dvm;
        }

        public async Task UserConfirmDisbursement(string disburseID)
        {
            Disbursement disbursement = db.Disbursements.Find(disburseID);
            if(disbursement != null)
            {
                disbursement.status = "Collection Confirmed";
                await db.SaveChangesAsync();
            }
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            Requisition req = db.Requisitions.Where(x => x.disbursementId == disburseID).First();
            string deptID = req.departmentId;
            string sender = db.Retrievals.Find(req.retrievalId).clerkId;
            string linkref = "http://" + ConstantsConfig.linkrefURLPartial + "/Disbursement/DisbursementDetailForRep?disburseID=" + disburseID;
            emailBizLogic.SendEmail("confirmDisburse", null, sender, deptID, null, linkref);
            await Task.CompletedTask;
        }

        public List<Disbursement> GetDisbursementListForUser(HttpSessionStateBase session)
        {
            List<Disbursement> disburseList = new List<Disbursement>();
            Dictionary<string,string> empList = (Dictionary<string, string>) session["EmployeeList"];
            List<Disbursement> fulldisburseList = db.Disbursements.ToList();
            //HOD login
            if (session["HeadID"] is null)
            {
                foreach (Disbursement x in fulldisburseList)
                {
                    if(empList[x.repID] !=null)
                    {
                        disburseList.Add(x);
                    }
                }
                return disburseList;
            }
            //Requestors
            foreach (Disbursement x in fulldisburseList)
            {
                if(x.repID == session["EmployeeID"].ToString())
                {
                    disburseList.Add(x);
                    continue;
                }
                Requisition req = db.Requisitions.Where(z => z.disbursementId == x.disbursementId).First();
                string requestorID = req == null ? null : req.employee;
                if (requestorID !=null)
                {
                    bool IsRequestor = requestorID == session["EmployeeID"].ToString() ? true : false;
                    if(IsRequestor)
                    {
                        disburseList.Add(x);
                    }                    
                }
            }
            return disburseList;
        }

    }
}


