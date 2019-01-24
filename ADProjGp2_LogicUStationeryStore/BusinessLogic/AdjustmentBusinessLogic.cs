using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;
using Newtonsoft.Json;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class AdjustmentBusinessLogic : Controller
    {
        SSISEntities db = new SSISEntities();
        public AdjustmentVoucherViewModel InitiateAdjustmentVoucher(HttpSessionStateBase session)
        {
            //Adjustment Voucher creation page first landing
            AdjustmentVoucherViewModel avvm = new AdjustmentVoucherViewModel();
            Dictionary<string, string> storeEmpList = GetStoreEmployeeList(session["Username"].ToString(), session["Password"].ToString());
            DateTime timenow = DateTime.Now;
            string timestamp = timenow.ToString("yyyyMMddhhmmss");
            string storeClerkID = session["EmployeeID"].ToString();
            avvm.voucherID = "ADJ" + storeClerkID + timestamp;
            avvm.voucherDate = timenow;
            avvm.clerkIDName = session["EmployeeName"].ToString() + " (" + storeClerkID + ")";
            avvm.supervisorIDName = storeEmpList["supervisor"];
            avvm.managerIDName = storeEmpList["manager"];
            avvm.needAuthority = false;
            avvm.status = "Pending";
            avvm.itemList = new List<AdjustmentVoucherViewModelDetail>();
            return avvm;
        }

        public AdjustmentVoucherViewModel AddItem(AdjustmentVoucherViewModel avvm, string itemIDComposite, int ItemQty)
        {
            string itemID = itemIDComposite.Substring(itemIDComposite.Length - 4, 4);
            if (avvm.itemList.Any())
            {
                if (avvm.itemList.Where(x => x.itemID == itemID).Any())
                {
                    AdjustmentVoucherViewModelDetail oldItem = avvm.itemList.Where(x => x.itemID == itemID).First();
                    if (oldItem != null)
                    {
                        oldItem.itemQty += ItemQty;
                        return null;
                    }
                }
            }
            //prepare the substring since the addition is composite (only last 4 character is the itemcode)
            
            Catalogue catalog = db.Catalogues.Find(itemID);
            if(catalog is null)
            {
                return null;
            }
            AdjustmentVoucherViewModelDetail newItem = new AdjustmentVoucherViewModelDetail();
            newItem.itemID = itemID;
            newItem.itemDescription = catalog.description;
            newItem.itemInventoryLocation = 1;
            newItem.itemQty = ItemQty;
            newItem.itemStoreQty = db.Inventories.Find(itemID).storeQuantity;
            newItem.itemDisburseQty = db.Inventories.Find(itemID).disburseQuantity;
            newItem.remark = null;
            avvm.itemList.Add(newItem);
            AdjustmentVoucherViewModel newavvm = avvm;
            return newavvm;
        }

        public void DeleteItem(AdjustmentVoucherViewModel avvm, string itemIDComposite)
        {
            string itemID = itemIDComposite.Substring(itemIDComposite.Length - 4, 4);
            if (!avvm.itemList.Any())
            {
                //nothing to delete
                return;
            }
            else
            {
                if (avvm.itemList.Where(x => x.itemID == itemID).Any())
                {
                    AdjustmentVoucherViewModelDetail avvmd = avvm.itemList.Where(x => x.itemID == itemID).First();
                    avvm.itemList.Remove(avvmd);
                }
                return;
            }
        }

        public string CreateAdjustmentVoucher(AdjustmentVoucherViewModel avvm)
        {
            //ItemInventoryLocation - 1 - StockQty, 2 - DisburseQty, 3 - AdjustQty (not used)
            decimal totalAdjAmt = 0M;

            //Initiate Adjustment entry
            Adjustment adjustment = new Adjustment();
            adjustment.voucherId = avvm.voucherID;
            adjustment.clerk = new string(avvm.clerkIDName.Where(Char.IsDigit).ToArray());
            adjustment.supervisor = new string(avvm.supervisorIDName.Where(Char.IsDigit).ToArray());
            adjustment.date = avvm.voucherDate;
            adjustment.status = "Submitted";

            foreach (AdjustmentVoucherViewModelDetail avvmd in avvm.itemList)
            {
                AdjustmentDetail adjustmentDetail = new AdjustmentDetail();
                Inventory inventory = db.Inventories.Find(avvmd.itemID);

                //edit Adj detail
                adjustmentDetail.itemId = avvmd.itemID;
                adjustmentDetail.quantity = avvmd.itemQty;
                adjustmentDetail.voucherId = avvm.voucherID;
                if (avvmd.itemInventoryLocation == 1)
                {
                    //Adj inventory
                    inventory.storeQuantity = avvmd.itemQty < 0 ? inventory.storeQuantity - Math.Abs(avvmd.itemQty) : inventory.storeQuantity + avvmd.itemQty;
                    inventory.adjQuantity -= avvmd.itemQty;
                    avvmd.remark += (" Adjusted Item code : " + avvmd.itemID + " Qty : " + avvmd.itemQty + " from StoreQty");
                }
                else if(avvmd.itemInventoryLocation == 2)
                {
                    inventory.disburseQuantity += avvmd.itemQty < 0 ? inventory.disburseQuantity - Math.Abs(avvmd.itemQty) : inventory.disburseQuantity + avvmd.itemQty;
                    inventory.adjQuantity -= avvmd.itemQty;
                    avvmd.remark += (" Adjusted Item code : " + avvmd.itemID + " Qty : " + avvmd.itemQty + " from DisburseQty");
                }
                string supplier = db.Catalogues.Find(avvmd.itemID).firstSupplier;
                decimal itemPrice = db.SupplierQuotations.Where(x => x.itemId == avvmd.itemID && x.supplierId == supplier).First().price;
                totalAdjAmt += (itemPrice * avvmd.itemQty);
                adjustmentDetail.remark = avvmd.remark;
                db.AdjustmentDetails.Add(adjustmentDetail);
            }
            avvm.needAuthority = totalAdjAmt > 250M ? true : false;
            avvm.status = "Submitted";

            //Set need authority (Logic - if need manager approval, set manager id - else null)
            adjustment.needAuthority = avvm.needAuthority == true ? new string(avvm.managerIDName.Where(Char.IsDigit).ToArray()) : null;
            adjustment.adjustmentValue = totalAdjAmt;
            db.Adjustments.Add(adjustment);
            db.SaveChanges();
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string receipient = avvm.needAuthority == true ? adjustment.needAuthority : adjustment.supervisor;
            string sender = adjustment.clerk;
            string linkref = "http://" + ConstantsConfig.linkrefURLPartial + "/Adjustment/AdjustmentDetail?adjustmentID=" + avvm.voucherID;
            emailBizLogic.SendEmail("submitAdj", receipient, sender, null, null, linkref);
            return adjustment.voucherId;
        }

        public List<AdjustmentVoucherViewModel> GetAdjustmentHistory(HttpSessionStateBase session)
        {
            List<AdjustmentVoucherViewModel> adjustmentsList = new List<AdjustmentVoucherViewModel>();
            List<string> adjIDs = db.Adjustments.Select(x => x.voucherId).ToList();
            foreach(string adjID in adjIDs)
            {
                adjustmentsList.Add(GetAdjustmentVoucher(adjID, session));
            }
            return adjustmentsList;
        }

        public AdjustmentVoucherViewModel GetAdjustmentVoucher(string adjustmentID, HttpSessionStateBase session)
        {
            if(adjustmentID !=null)
            {
                if (db.Adjustments.Find(adjustmentID) !=null)
                {
                    Adjustment adjustment = db.Adjustments.Find(adjustmentID);
                    AdjustmentVoucherViewModel avvm = new AdjustmentVoucherViewModel();
                    Dictionary<string, string> storeEmpList = GetStoreEmployeeList("mstan", "mstan");                    
                    avvm.clerkIDName = storeEmpList["clerk"];
                    avvm.supervisorIDName = storeEmpList["supervisor"];
                    avvm.managerIDName = storeEmpList["manager"];
                    avvm.status = adjustment.status;
                    avvm.needAuthority = adjustment.needAuthority == null ? false : true;
                    avvm.voucherDate = adjustment.date;
                    avvm.voucherID = adjustment.voucherId;
                    avvm.adjustmentValue = adjustment.adjustmentValue;
                    avvm.itemList = new List<AdjustmentVoucherViewModelDetail>();
                    List<AdjustmentDetail> adjustmentDetails = db.AdjustmentDetails.Where(x => x.voucherId == adjustmentID).ToList();
                    foreach(AdjustmentDetail adjDet in adjustmentDetails)
                    {
                        AdjustmentVoucherViewModelDetail avvmd = new AdjustmentVoucherViewModelDetail();
                        avvmd.itemID = adjDet.itemId;
                        avvmd.itemDescription = db.Catalogues.Find(adjDet.itemId).description;
                        avvmd.itemQty = adjDet.quantity;
                        avvmd.remark = adjDet.remark;
                        avvm.itemList.Add(avvmd);
                    }
                    return avvm;
                }
                return null;
            }
            return null;
        }

        public AdjustmentVoucherViewModel ApproveVoucher(AdjustmentVoucherViewModel avvm)
        {
            Adjustment adjustment = db.Adjustments.Find(avvm.voucherID);
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            if (avvm.needAuthority == true)
            {
                adjustment.status = "Approved by " + avvm.managerIDName;
                avvm.status = "Approved by " + avvm.managerIDName;
            }
            else
            {
                adjustment.status = "Approved by " + avvm.supervisorIDName;
                avvm.status = "Approved by " + avvm.supervisorIDName;
            }
            db.SaveChanges();
            string receipient = adjustment.clerk;
            string sender = avvm.needAuthority == true ? adjustment.needAuthority : adjustment.supervisor;
            string linkref = "http://" + ConstantsConfig.linkrefURLPartial + "/Adjustment/AdjustmentDetail?adjustmentID=" + avvm.voucherID;
            emailBizLogic.SendEmail("approveAdj", receipient, sender, null, null, linkref);
            return avvm;
        }

        public DisbursementViewModel CreateDisbursementAdjustmentVoucher(DisbursementViewModel model, HttpSessionStateBase session)
        {
            Adjustment adjustment = new Adjustment();
            DateTime date = DateTime.Now;
            adjustment.date = date.Date;
            string timestamp = date.ToString("yyyyMMddhhmmss");
            adjustment.clerk = session["EmployeeID"].ToString();
            adjustment.status = "Submitted";
            adjustment.supervisor = session["HeadID"].ToString();
            adjustment.voucherId = "ADJ" + adjustment.clerk + timestamp;
            string disburseID = model.disbursementID;
            decimal totalvalue = 0;
            Dictionary<string, string> storeEmpList = GetStoreEmployeeList(session["Username"].ToString(), session["Password"].ToString());
            string managerIDName = storeEmpList["manager"];

            foreach (DisbursementViewModelDetail dvmd in model.dvmdList)
            {
                if(dvmd.adjQty == 0 || dvmd.adjQty == null)
                {
                    continue;
                }
                AdjustmentDetail adjDetail = new AdjustmentDetail();
                adjDetail.itemId = dvmd.itemID;
                adjDetail.quantity = (int) dvmd.adjQty;
                adjDetail.remark = "Disbursement (" + disburseID + ") Adjustment"; 
                adjDetail.voucherId = adjustment.voucherId;
                string supplier = db.Catalogues.Find(dvmd.itemID).firstSupplier;
                decimal itemPrice = db.SupplierQuotations.Where(x => x.itemId == dvmd.itemID && x.supplierId == supplier).First().price;
                totalvalue += (itemPrice * adjDetail.quantity);
                db.AdjustmentDetails.Add(adjDetail);
            }
            //Set need authority (Logic - if need manager approval, set manager id - else null)
            bool needAuthority = totalvalue > 250M ? true : false; ;
            adjustment.needAuthority = needAuthority == true ? new string(managerIDName.Where(Char.IsDigit).ToArray()) : null;
            adjustment.adjustmentValue = totalvalue;
            db.Adjustments.Add(adjustment);
            Disbursement disbursement = db.Disbursements.Find(disburseID);
            disbursement.adjustmentID = adjustment.voucherId;
            db.SaveChanges();

            model.status = "Adjustment Performed";
            model.adjustmentID = adjustment.voucherId;

            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string receipient = needAuthority ? adjustment.needAuthority : adjustment.supervisor;
            string sender = adjustment.clerk;
            string linkref = "http://" + ConstantsConfig.linkrefURLPartial + "/Adjustment/AdjustmentDetail?adjustmentID=" + adjustment.voucherId;
            emailBizLogic.SendEmail("submitAdj", receipient, sender, null, null, linkref);

            return model;
        }

        public Dictionary<string, string> GetStoreEmployeeList(string username, string password)
        {
            List<KeyValuePair<string, string>> storeEmployeeList = new List<KeyValuePair<string, string>>();
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://" + ConstantsConfig.ipaddress + "/LoginServiceIIS.svc/StoreEmployeeList?username=" + username + "&password=" + password);
                try
                {
                    storeEmployeeList = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(json);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            if (storeEmployeeList is null)
            {
                return null;
            }
            else if (storeEmployeeList.Count <= 0)
            {
                return null;
            }
            else
            {
                Dictionary<string, string> empList = storeEmployeeList.ToDictionary(x => x.Key, x => x.Value);
                return empList;
            }
        }

        public Dictionary<string,int> AdjItemsConversion(List<StockTakeModel> stmList)
        {
            Dictionary<string, int> adjitems = new Dictionary<string, int>();
            foreach(StockTakeModel stm in stmList)
            {
                foreach(StockTakeModelDetails stmd in stm.stmdList)
                {
                    if(stmd.UserAdjQty !=0)
                    {
                        adjitems.Add(stmd.itemID, stmd.UserAdjQty);
                    }
                }
            }
            return adjitems;
        }

        public string CreateStockTakeAdjustmentVoucher(HttpSessionStateBase session, Dictionary<string,int> adjitems)
        {
            AdjustmentVoucherViewModel avvm = InitiateAdjustmentVoucher(session);
            string itemID = "";
            int adjQty = 0;
            foreach (KeyValuePair<string,int> kvp in adjitems)
            {
                itemID = kvp.Key;
                adjQty = kvp.Value;
                avvm = AddItem(avvm, itemID, adjQty);
            }
            string AdjID = CreateAdjustmentVoucher(avvm);
            return AdjID;
        }

    }
}