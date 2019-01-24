using ADProjGp2_LogicUStationeryStore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class CommonBusinessLogic : Controller
    {
        private SSISEntities db = new SSISEntities();
        public LoginObjectModel ValidateUser(LoginObjectModel model)
        {
            string username = model.userName;
            string password = model.password;
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://"+ConstantsConfig.ipaddress+"/LoginServiceIIS.svc/LoginGet?username=" + username + "&password=" + password);
                try
                {
                    model = JsonConvert.DeserializeObject<LoginObjectModel>(json);
                    return model;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public ChangePasswordModel ChangePassword(ChangePasswordModel model)
        {
            string username = model.userName;
            string oldpassword = model.oldpassword;
            string newpassword = model.newpassword;
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://" + ConstantsConfig.ipaddress + "/LoginServiceIIS.svc/ChangePassword?username=" + username + "&oldpassword=" + oldpassword + "&newpassword=" + newpassword);
                try
                {
                    model = JsonConvert.DeserializeObject<ChangePasswordModel>(json);
                    return model;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public void PrepareSession(LoginObjectModel lom, Dictionary<string, string> empList, HttpSessionStateBase session)
        {
            session["Role"] = lom.role;
            session["HeadID"] = lom.superiorID;
            session["HeadName"] = lom.superiorName;
            session["EmployeeName"] = lom.employeeName;
            session["EmployeeID"] = lom.employeeId;
            session["DepartmentID"] = lom.departmentId;
            session["DepartmentName"] = lom.departmentName;
            session["Username"] = lom.userName;
            session["Password"] = lom.password;
            session["EmployeeList"] = empList;
            session["MessageBack"] = "";
        }

        // Prepare the Department Employees list menu in Authorisation and Representative form (JSON Array for JS)
        public string[] PrepareEmployeeMenu(Dictionary<string, string> emplist, string headID, HttpSessionStateBase session)
        {
            string[] empList = new string[emplist.Count - 1];
            string empString = "";
            int counter = 0;
            foreach (KeyValuePair<string, string> x in emplist)
            {
                if (x.Key != headID)
                {
                    empString = x.Value + " (Employee ID: " + x.Key + ")";
                    empList[counter] = empString;
                    counter++;
                }
            }
            //for verification purpose later when authorising
            session["AuthEmployeeList"] = empList;
            return empList;
        }

        public bool SetRepresentative(string repname, HttpSessionStateBase session)
        {
            if (repname is null)
            {
                session["MessageBack"] = "Please choose an employee to represent";
                return false;
            }
            string[] emplist = (string[])session["AuthEmployeeList"];
            if (!emplist.Contains(repname))
            {
                session["MessageBack"] = "No such employee (" + repname + ") in this department";
                return false;
            }
            session["MessageBack"] = "";
            string deptid = session["DepartmentID"].ToString();
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptid).First();
            string newrepID = new string(repname.Where(Char.IsDigit).ToArray());
            string username = session["Username"].ToString();
            string password = session["Password"].ToString();
            string statusMessage = WebAccessSetRepresentative(username, password, newrepID);
            if (statusMessage == "Success")
            {
                deptDetails.representative = newrepID;
                EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
                string sender = session["EmployeeID"].ToString();
                string receipient = newrepID;
                emailBizLogic.SendEmail("assignRep", receipient, sender, null, null, null);
            }
            db.SaveChangesAsync();
            return true;
        }

        public string SetRepresentativeWebAPI(string newrepid, string deptID, string headID, string username, string password)
        {
            Dictionary<string, string> emplist = GetEmployeeList(username, password, deptID);
            if (newrepid is null)
            {
                return "Please choose an employee to represent";
            }
            if (!emplist.ContainsKey(newrepid))
            {
                return "No such employee of id:" + newrepid + " in this department";
            }
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            string statusMessage = WebAccessSetRepresentative(username, password, newrepid);
            if (statusMessage == "Success")
            {
                deptDetails.representative = newrepid;
                EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
                string sender = headID;
                string receipient = newrepid;
                emailBizLogic.SendEmail("assignRep", receipient, sender, null, null, null);
            }
            db.SaveChangesAsync();
            return "Success";
        }

        public async Task CancelRepresentative(string deptId, HttpSessionStateBase session)
        {
            if(db.DeptCollectionDetails.Find(deptId).representative == null)
            {
                return;
            }
            string deptid = session["DepartmentID"].ToString();
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptid).First();
            string newrepID = "";
            string username = session["Username"].ToString();
            string password = session["Password"].ToString();
            string statusMessage = WebAccessSetRepresentative(username, password, newrepID);
            if (statusMessage == "Success")
            {
                string removedRep = db.DeptCollectionDetails.Find(deptId).representative;
                db.DeptCollectionDetails.Find(deptId).representative = null;
                EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
                string sender = session["EmployeeID"].ToString();
                string receipient = removedRep;
                emailBizLogic.SendEmail("removeRep", receipient, sender, null, null, null);
            }
            else
            {
                return;
            }
            await db.SaveChangesAsync();
        }

        public string CancelRepresentativeWebAPI(string deptID, string headID, string username, string password)
        {
            if (db.DeptCollectionDetails.Find(deptID).representative == null)
            {
                return "Success";
            }
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            string newrepID = "";
            string statusMessage = WebAccessSetRepresentative(username, password, newrepID);
            if (statusMessage == "Success")
            {
                string removedRep = db.DeptCollectionDetails.Find(deptID).representative;
                db.DeptCollectionDetails.Find(deptID).representative = null;
                EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
                string sender = headID;
                string receipient = removedRep;
                emailBizLogic.SendEmail("removeRep", receipient, sender, null, null, null);
            }
            else
            {
                return statusMessage;
            }
            db.SaveChangesAsync();
            return "Success";
        }

        public string WebAccessSetRepresentative(string username, string password, string newrepid)
        {
            string statusMessage = "Fail";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString("http://" + ConstantsConfig.ipaddress + "/LoginServiceIIS.svc/SetRepresentative?username=" + username + "&password=" + password + "&newrepid=" + newrepid);
                    try
                    {
                        statusMessage = JsonConvert.DeserializeObject<string>(json);
                    }
                    catch (Exception e)
                    {
                        return statusMessage;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage;
            }
        }

        public bool SetAuthorisedPerson(string authperson, string startdate, string enddate, HttpSessionStateBase session)
        {
            if (authperson is null || startdate is null || enddate is null)
            {
                session["MessageBack"] = "Please enter all necessary fields";
                return false;
            }
            string[] emplist = (string[])session["AuthEmployeeList"];
            if (!emplist.Contains(authperson))
            {
                session["MessageBack"] = "No such employee (" + authperson + ") in this department";
                return false;
            }
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            DateTime.TryParse(startdate, out startDate);
            DateTime.TryParse(enddate, out endDate);
            DateTime compareDate = DateTime.Now.Date;

            if (startDate < compareDate || endDate < compareDate)
            {
                session["MessageBack"] = "Please ensure both start date (" + startdate + ") and end date (" + enddate + ") are not earlier the date today (" + compareDate.ToString() + ")";
                return false;
            }
            if (startDate > endDate)
            {
                session["MessageBack"] = "Your selected start date " + startdate + "is earlier than your selected end date " + enddate;
                return false;
            }
            session["MessageBack"] = "";
            string deptid = session["DepartmentID"].ToString();
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptid).First();
            string newempID = new string(authperson.Where(Char.IsDigit).ToArray());
            string username = session["Username"].ToString();
            string password = session["Password"].ToString();
            string statusMessage = WebAccessSetAuthorisedPerson(username, password, newempID);
            if (statusMessage == "Success")
            {
                deptDetails.authorisedPerson = newempID;
                deptDetails.validDateStart = startDate;
                deptDetails.validDateEnd = endDate;
            }
            else
            {
                return false;
            }
            db.SaveChanges();
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string sender = session["EmployeeID"].ToString();
            string receipient = newempID;
            emailBizLogic.SendEmail("assignAuth", receipient, sender, null, null, null);
            return true;
        }

        public string SetAuthorisedPersonWebAPI(string authperson, string headID, string deptID, string startdate, string enddate, string username, string password)
        {
            Dictionary<string, string> emplist = GetEmployeeList(username, password, deptID);

            if (deptID is null || startdate is null || enddate is null)
            {
                return "Important Parameters cannot be null";
            }
            if (!emplist.ContainsKey(authperson))
            {
                return "Please ensure that employee authorised is in your department";
            }
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            bool date1pass = DateTime.TryParse(startdate, out startDate);
            bool date2pass = DateTime.TryParse(enddate, out endDate);
            DateTime compareDate = DateTime.Now.Date;
            if(!date1pass || !date2pass)
            {
                return "Please ensure dates are in valid format";
            }
            if (startDate < compareDate || endDate < compareDate)
            {               
                return "Please ensure both start date (" + startdate + ") and end date (" + enddate + ") are not earlier the date today (" + compareDate.ToString() + ")";
            }
            if (startDate > endDate)
            {
                return "Your selected start date " + startdate + "is earlier than your selected end date " + enddate;
            }           
            
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            string statusMessage = WebAccessSetAuthorisedPerson(username, password, authperson);
            if (statusMessage == "Success")
            {
                deptDetails.authorisedPerson = authperson;
                deptDetails.validDateStart = startDate;
                deptDetails.validDateEnd = endDate;
            }
            else
            {
                return statusMessage;
            }
            db.SaveChanges();
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string sender = headID;
            string receipient = authperson;
            emailBizLogic.SendEmail("assignAuth", receipient, sender, null, null, null);
            return "ok";
        }

        public async Task CancelAuthorisedPerson(HttpSessionStateBase session)
        {
            string deptid = session["DepartmentID"].ToString();
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptid).First();
            if(deptDetails.authorisedPerson == null)
            {
                return;
            }
            string newempID = "";
            string username = session["Username"].ToString();
            string password = session["Password"].ToString();
            string statusMessage = WebAccessSetAuthorisedPerson(username, password, newempID);
            if (statusMessage == "Success")
            {
                string removedAuth = db.DeptCollectionDetails.Find(deptid).authorisedPerson;
                EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
                string sender = session["EmployeeID"].ToString();
                string receipient = removedAuth;
                emailBizLogic.SendEmail("removeAuth", receipient, sender, null, null, null);
                db.DeptCollectionDetails.Find(deptid).authorisedPerson = null;
                db.DeptCollectionDetails.Find(deptid).validDateStart = null;
                db.DeptCollectionDetails.Find(deptid).validDateEnd = null;
            }
            else
            {
                return;
            }
            await db.SaveChangesAsync();
        }

        public string CancelAuthorisedPersonWebAPI(string headID, string deptID, string username, string password)
        {            
            DeptCollectionDetail deptDetails = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            if(deptDetails == null)
            {                
                return "No such department exits";
            }
            else
            {
                if(deptDetails.authorisedPerson == null)
                {
                    return "Success";
                }
            }
            string newempID = "";
            string statusMessage = WebAccessSetAuthorisedPerson(username, password, newempID);
            if (statusMessage == "Success")
            {
                string removedAuth = db.DeptCollectionDetails.Find(deptID).authorisedPerson;
                EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
                string sender = headID;
                string receipient = removedAuth;
                emailBizLogic.SendEmail("removeAuth", receipient, sender, null, null, null);
                db.DeptCollectionDetails.Find(deptID).authorisedPerson = null;
                db.DeptCollectionDetails.Find(deptID).validDateStart = null;
                db.DeptCollectionDetails.Find(deptID).validDateEnd = null;
            }
            else
            {
                return "Fail";
            }
            db.SaveChangesAsync();
            return "Success";
        }


        public string WebAccessSetAuthorisedPerson(string username, string password, string newpersonid)
        {
            string statusMessage = "Fail";
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://" + ConstantsConfig.ipaddress + "/LoginServiceIIS.svc/SetAuthorisePerson?username=" + username + "&password=" + password + "&newpersonid=" + newpersonid);
                try
                {
                    statusMessage = JsonConvert.DeserializeObject<string>(json);
                }
                catch (Exception e)
                {
                    return statusMessage;
                }
                return statusMessage;
            }
        }

        public Dictionary<string, string> GetAuthPersonRep(string deptID, string HeadId)
        {
            Dictionary<string, string> valuePairs = new Dictionary<string, string>();
            if (db.DeptCollectionDetails.Find(deptID).authorisedPerson is null)
            {
                valuePairs.Add("AuthorisedPax", HeadId);
                valuePairs.Add("ValidStart", null);
                valuePairs.Add("ValidEnd", null);
            }
            else
            {
                valuePairs.Add("AuthorisedPax", db.DeptCollectionDetails.Find(deptID).authorisedPerson);
                valuePairs.Add("ValidStart", db.DeptCollectionDetails.Find(deptID).validDateStart.ToString());
                valuePairs.Add("ValidEnd", db.DeptCollectionDetails.Find(deptID).validDateEnd.ToString());
            }
            valuePairs.Add("Representative", db.DeptCollectionDetails.Find(deptID).representative);
            valuePairs.Add("DepartmentName", db.DeptCollectionDetails.Find(deptID).departmentName);
            valuePairs.Add("CollectionPoint", db.DeptCollectionDetails.Find(deptID).collectionPoint);

            return valuePairs;
        }

        public Dictionary<string, string> GetEmployeeList(string username, string password, string deptID)
        {
            List<KeyValuePair<string, string>> deptEmployeeList = new List<KeyValuePair<string, string>>();
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://" + ConstantsConfig.ipaddress + "/LoginServiceIIS.svc/DepartmentEmployeeList?username=" + username + "&password=" + password + "&departmentID=" + deptID);
                try
                {
                    deptEmployeeList = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(json);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            if (deptEmployeeList is null)
            {
                return null;
            }
            else if (deptEmployeeList.Count <= 0)
            {
                return null;
            }
            else
            {
                Dictionary<string, string> empList = deptEmployeeList.ToDictionary(x => x.Key, x => x.Value);
                return empList;
            }
        }

        public Dictionary<string, string> RetrieveCurrentCollectionInfo(string deptID, Dictionary<string, string> tempdata)
        {
            DeptCollectionDetail deptCollectionDetail = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            CollectionPoint collectPoint = db.CollectionPoints.Where(y => y.locationName == deptCollectionDetail.collectionPoint).First();
            tempdata.Add("CollectPoint", deptCollectionDetail.collectionPoint);
            tempdata.Add("CollectTime", collectPoint.collectTime);
            return tempdata;
        }

        public async Task SetCurrentCollectionInfo(string deptID, string selection)
        {
            DeptCollectionDetail deptCollectionDetail = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            List<CollectionPoint> collectionPoints = db.CollectionPoints.ToList<CollectionPoint>();
            foreach (CollectionPoint x in collectionPoints)
            {
                if (selection.Contains(x.locationName))
                {
                    deptCollectionDetail.collectionPoint = x.locationName;
                    break;
                }
            }
            await db.SaveChangesAsync();
        }

        public List<string> GetLocationsNTimeList()
        {
            string prepstring = "";
            List<string> locationsNTime = new List<string>();
            List<CollectionPoint> locationsNTimeObj = db.CollectionPoints.ToList<CollectionPoint>();
            foreach (CollectionPoint x in locationsNTimeObj)
            {
                prepstring = x.locationName + " ( " + x.collectTime + " )";
                locationsNTime.Add(prepstring);
            }
            return locationsNTime;
        }

        public ClerkWelcomePageModel GetClerkWelcomePageModel (HttpSessionStateBase session)
        {
            Dictionary<string,string> emplist = (Dictionary<string,string>) session["EmployeeList"];
            ClerkWelcomePageModel model = new ClerkWelcomePageModel();
            model.AdjustmentWPC = new List<AdjustmentWelcomePageComponent>();
            model.DisbursementWPC = new List<DisbursementWelcomePageComponent>();
            model.RequisitionWPC = new List<RequisitionWelcomePageComponent>();
            model.RetrievalWPC = new List<RetrievalWelcomePageComponent>();
            model.StockWPC = GetLowStockLevels();

            foreach(Requisition req in db.Requisitions)
            {
                if (req.status == "Approved")
                {
                    RequisitionWelcomePageComponent reqWPC = new RequisitionWelcomePageComponent();
                    reqWPC.RequisitionID = req.requisitionId;
                    reqWPC.RequisitionCreator = req.employee;
                    reqWPC.RequisitionStatus = req.status;
                    reqWPC.RequisitionApproveDate = req.approvalDate == null? null : req.approvalDate.ToString();
                    model.RequisitionWPC.Add(reqWPC);
                }
            }
            foreach (Retrieval ret in db.Retrievals)
            {
                if (ret.status == "Pending")
                {
                    RetrievalWelcomePageComponent retWPC = new RetrievalWelcomePageComponent();
                    retWPC.RetrievalID = ret.retrievalId;
                    retWPC.RetrievalCreationDate = ret.retrievalCreationDate.ToShortDateString();
                    retWPC.RetrievalCreator = ret.clerkId;
                    retWPC.RetrievalStatus = ret.status;
                    model.RetrievalWPC.Add(retWPC);
                }
            }
            foreach (Disbursement disburse in db.Disbursements)
            {
                if (disburse.status == "Awaiting Collection")
                {
                    DisbursementWelcomePageComponent disWPC = new DisbursementWelcomePageComponent();
                    disWPC.DisbursementID = disburse.disbursementId;
                    disWPC.DisbursementStatus = disburse.status;
                    disWPC.DisbursementCreationDate = disburse.disburseDate == null ? null: disburse.disburseDate.ToString();
                    disWPC.DisbursementRep = disburse.repID != null ? emplist[disburse.repID] : null;
                    Requisition requisitionDis = db.Requisitions.Where(x => x.disbursementId == disburse.disbursementId).FirstOrDefault();
                    if(requisitionDis !=null)
                    {
                        disWPC.DisbursementDept = requisitionDis.departmentId;
                        disWPC.DisbursementRetID = requisitionDis.retrievalId;
                        Retrieval retDis = db.Retrievals.Find(requisitionDis.retrievalId);
                        if (retDis != null)
                        {                            
                            disWPC.DisbursementCreator = retDis.clerkId;
                        }
                        disWPC.DisburementCollectionPoint = db.DeptCollectionDetails.Find(disWPC.DisbursementDept).collectionPoint;
                        DateTime today = DateTime.Today;
                        if(today.DayOfWeek == DayOfWeek.Monday)
                        {
                            disWPC.DisbursementCollectDate = "Collection is today, " + today.ToString("dddd, dd MMMM yyyy");
                        }
                        else if (today.DayOfWeek == DayOfWeek.Sunday)
                        {
                            disWPC.DisbursementCollectDate = "Collection is tomorrow, " + today.ToString("dddd, dd MMMM yyyy");
                        }
                        else
                        {
                            int startInt = 8;
                            int daysToCome = startInt - (int)today.DayOfWeek;
                            disWPC.DisbursementCollectDate = "Collection is in " +daysToCome +" more days, on " + today.ToString("dddd, dd MMMM yyyy");
                        }
                    }
                    model.DisbursementWPC.Add(disWPC);
                }
            }

            foreach (Adjustment adj in db.Adjustments)
            {
                if (adj.status == "Submitted" & adj.clerk == session["EmployeeID"].ToString())
                {
                    AdjustmentWelcomePageComponent adjWPC = new AdjustmentWelcomePageComponent();
                    adjWPC.AdjustmentID = adj.voucherId;
                    adjWPC.AdjustmentDate = adj.date.ToShortDateString();
                    adjWPC.AdjustmentCreator = adj.clerk;
                    adjWPC.AdjustmentStatus = adj.status;
                    model.AdjustmentWPC.Add(adjWPC);
                }
            }

            return model;
        }

        public SupervisorWelcomePageModel GetSupervisorWelcomePageModel(HttpSessionStateBase session)
        {
            SupervisorWelcomePageModel model = new SupervisorWelcomePageModel();
            model.AdjustmentWPC = new List<AdjustmentWelcomePageComponent>();

            foreach (Adjustment adj in db.Adjustments)
            {
                if (adj.status == "Submitted" && adj.supervisor == session["EmployeeID"].ToString() && adj.needAuthority == null)
                {
                    AdjustmentWelcomePageComponent adjWPC = new AdjustmentWelcomePageComponent();
                    adjWPC.AdjustmentID = adj.voucherId;
                    adjWPC.AdjustmentDate = adj.date.ToShortDateString();
                    adjWPC.AdjustmentCreator = adj.clerk;
                    adjWPC.AdjustmentStatus = adj.status;
                    model.AdjustmentWPC.Add(adjWPC);
                }
            }
            return model;
        }

        public ManagerWelcomePageModel GetManagerWelcomePageModel(HttpSessionStateBase session)
        {
            ManagerWelcomePageModel model = new ManagerWelcomePageModel();
            model.AdjustmentWPC = new List<AdjustmentWelcomePageComponent>();

            foreach (Adjustment adj in db.Adjustments)
            {
                if (adj.status == "Submitted" && adj.needAuthority == session["EmployeeID"].ToString())
                {
                    AdjustmentWelcomePageComponent adjWPC = new AdjustmentWelcomePageComponent();
                    adjWPC.AdjustmentID = adj.voucherId;
                    adjWPC.AdjustmentDate = adj.date.ToShortDateString();
                    adjWPC.AdjustmentCreator = adj.clerk;
                    adjWPC.AdjustmentStatus = adj.status;
                    model.AdjustmentWPC.Add(adjWPC);
                }
            }
            return model;
        }

        public List<StockWarningComponent> GetLowStockLevels()
        {
            List<Catalogue> catList = db.Catalogues.ToList();
            List<StockWarningComponent> lowStockList = new List<StockWarningComponent>();
            foreach (Catalogue cat in catList)
            {
                Inventory inv = db.Inventories.Find(cat.itemId);
                if (cat.reorderLevel < inv.storeQuantity)
                {
                    //No need to reorder
                    continue;
                }

                bool conditionPurchaseMade = false;
                int purchaseAmt = 0;
                string PurchaseOrderID = "";
                List<PurchaseOrderDetail> PODs = db.PurchaseOrderDetails.Where(x => x.itemId == cat.itemId).ToList();
                if(PODs is null)
                {
                    conditionPurchaseMade = false;
                    foreach(PurchaseOrderDetail pod in PODs)
                    {
                        PurchaseOrder purchase = db.PurchaseOrders.Where(x => x.poId == pod.poId && x.remark == "Pending").FirstOrDefault();
                        if(purchase != null)
                        {
                            PurchaseOrderID = PurchaseOrderID == "" ? pod.poId : " " + pod.poId;
                            conditionPurchaseMade = true;
                            purchaseAmt += pod.quantity;
                        }
                    }
                }
                StockWarningComponent component = new StockWarningComponent();
                component.ItemID = cat.itemId;
                component.ItemDescription = cat.description;
                component.qtyStock = inv.storeQuantity;
                component.qtyReorderLevel = cat.reorderLevel;
                component.purchaseMade = conditionPurchaseMade;
                component.purchaseQty = purchaseAmt;
                lowStockList.Add(component);
            }
            return lowStockList;
        }

        public List<StockTakeModel> GenerateStocktake (string empID)
        {
            List<Catalogue> catList = db.Catalogues.ToList();
            catList.Sort(new CatalogueComparerByBin());
            DateTime today = DateTime.Now.Date;

            List<StockTakeModel> stockTakeList = new List<StockTakeModel>();
            foreach (Catalogue cat in catList)
            {
                StockTakeModelDetails stmd = new StockTakeModelDetails();
                stmd.itemID = cat.itemId;
                stmd.itemDescription = cat.description;
                stmd.catagory = cat.category;
                Inventory inv = db.Inventories.Find(cat.itemId);
                stmd.invQty = inv.storeQuantity;
                stmd.disburseQty = inv.disburseQuantity;
                stmd.adjQty = inv.adjQuantity;
                stmd.UserAdjQty = 0;
                StockTakeModel stm = new StockTakeModel();
                if (stockTakeList.Find(x=>x.binNumber == cat.bin)!=null)
               {
                    stm = stockTakeList.Find(x => x.binNumber == cat.bin);
                    stm.stmdList.Add(stmd);
                }
                else
                {
                    stm.stocktakeDate = today.ToShortDateString();
                    stm.stocktakeClerk = empID;
                    stm.binNumber = cat.bin;
                    stm.stmdList = new List<StockTakeModelDetails>();
                    stm.stmdList.Add(stmd);
                    stockTakeList.Add(stm);
                }
            }
            return stockTakeList;
        }
    }
}