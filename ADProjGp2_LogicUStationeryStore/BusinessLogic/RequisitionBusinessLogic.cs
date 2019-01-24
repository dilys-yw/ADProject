using ADProjGp2_LogicUStationeryStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    //Requisition Business Logic - All process logic dealing with requisition form Creation, Sumission and Approvals
    public class RequisitionBusinessLogic : Controller
    {
        private SSISEntities db = new SSISEntities();

        // Prepare the item menu in Requisition form (JSON Array for JS)
        public string[] PrepareItemMenu()
        {
            List<Catalogue> itemlist = db.Catalogues.ToList<Catalogue>();
            string[] menuitem = new string[itemlist.Count];
            string fullDescription = "";
            int counter = 0;
            foreach (Catalogue x in itemlist)
            {
                if (x.unitOfMeasure == "Each" || x.unitOfMeasure == "Dozen")
                {
                    fullDescription = x.description + "(" + x.unitOfMeasure + ")" + " - " + x.itemId;
                }
                else
                {
                    fullDescription = x.description + "(" + x.unitOfMeasure + " of " + x.quantityInUnit + ")" + " - " + x.itemId;
                }
                menuitem[counter] = fullDescription;
                counter++;
            }
            return menuitem;
        }

        public Requisition RequisitionFormGeneration(HttpSessionStateBase session, RequisitionFormItemCart itemcart)
        {
            DeptCollectionDetail dept = db.DeptCollectionDetails.ToList<DeptCollectionDetail>().Where(x => x.departmentId == session["DepartmentID"].ToString()).First();
            string authPerson = dept.authorisedPerson;
            Requisition requisition = new Requisition();

            if (authPerson != null && dept.validDateStart < DateTime.Now && dept.validDateEnd > DateTime.Now)
                requisition.approvalPerson = authPerson;
            else
                requisition.approvalPerson = session["HeadID"].ToString();

            requisition.employee = session["EmployeeID"].ToString();
            requisition.requisitionId = "REQ-" + session["DepartmentID"].ToString() + "-" + session["EmployeeID"].ToString() + "-" + DateTime.Now.ToString("yyMMddhhmmss");
            requisition.departmentId = session["DepartmentID"].ToString();
            requisition.RequisitionDetails = itemcart.RequestItemCart();

            foreach (RequisitionDetail x in requisition.RequisitionDetails)
            {
                x.requisitionId = requisition.requisitionId;
                x.transId = 0;
            }
            requisition.requestDate = DateTime.Now;
            return requisition;
        }

        public Requisition GetRequisitionDetails(Requisition requisition)
        {
            requisition.RequisitionDetails = new List<RequisitionDetail>();
            foreach (RequisitionDetail x in db.RequisitionDetails)
            {
                if(x.requisitionId == requisition.requisitionId)
                {
                    RequisitionDetail reqdet = new RequisitionDetail();
                    reqdet = x;
                    requisition.RequisitionDetails.Add(reqdet);
                }
            }
            return requisition;
        }

        public async Task<Requisition> SaveRequisitionForm(Requisition req, HttpSessionStateBase session)
        {
            req.status = "Submitted";
            db.Requisitions.Add(req);
            await db.SaveChangesAsync();
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string receipient= req.approvalPerson;
            string sender = req.employee;
            string linkref = "http://"+ConstantsConfig.linkrefURLPartial + "/Requisition/ReqFormDetail?id="+ req.requisitionId;
            emailBizLogic.SendEmail("submitNewReq", receipient, sender, null, null, linkref);
            return req;
        }


        public async Task<Requisition> ApproveRequisitionForm(Requisition req, HttpSessionStateBase session)
        {
            Requisition requisition = db.Requisitions.Where(x => x.requisitionId == req.requisitionId).First();
            string approvalPerson = session["EmployeeID"].ToString();
            requisition.approvalPerson = approvalPerson;
            requisition.status = "Approved";
            requisition.approvalDate = DateTime.Now;
            await db.SaveChangesAsync();
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string sender = approvalPerson;
            string receipient = req.employee;
            string linkref = "http://" + ConstantsConfig.linkrefURLPartial + "/Requisition/ReqFormDetail?id=" + req.requisitionId;
            emailBizLogic.SendEmail("updateReqStatus", receipient, sender, null, "Approved", linkref);
            emailBizLogic.SendEmail("sendStoreClerkReq", receipient, null, req.departmentId, null, linkref);
            return requisition;
        }

        public List<Requisition> MyRequisitionHistoryList(HttpSessionStateBase session)
        {
            List<Requisition> myReqList = new List<Requisition>();
            string empID = session["EmployeeID"].ToString();
            string deptID = session["DepartmentID"].ToString();
            if (session["Role"].ToString() == "head" || session["Role"].ToString().Contains("auth"))
            {
                myReqList = db.Requisitions.Where(x => x.departmentId == deptID).ToList<Requisition>();
            }
            else if (session["Role"].ToString() == "user" || session["Role"].ToString() == "rep")
            {
                myReqList = db.Requisitions.Where(x => x.departmentId == deptID).Where(x => x.employee == empID).ToList<Requisition>();
            }
            else if (session["Role"].ToString() == "clerk")
            {
                myReqList = db.Requisitions.Where(x => x.status != "Submitted" && x.status != "Rejected by head").ToList<Requisition>();
            }
            return myReqList;
        }

        public async Task<Requisition> RejectRequisitionForm(Requisition req, HttpSessionStateBase session)
        {
            Requisition requisition = db.Requisitions.Where(x => x.requisitionId == req.requisitionId).First();
            if (session["Role"].ToString() == "clerk")
            {
                requisition.status = "Rejected by store clerk";
            }
            else
            {
                requisition.status = "Rejected by head";
            }
            requisition.remark = req.remark;
            await db.SaveChangesAsync();
            EmailBusinessLogic emailBizLogic = new EmailBusinessLogic();
            string sender = session["EmployeeID"].ToString();
            string receipient = req.employee;
            string linkref = "http://" + ConstantsConfig.ipaddress + "/Requisition/ReqFormDetail?id=" + req.requisitionId;
            emailBizLogic.SendEmail("updateReqStatus", receipient, sender, null, "Rejected", linkref);
            return requisition;
        }

        public bool checkHeadValid(HttpSessionStateBase session)
        {
            string deptID = session["DepartmentID"].ToString();
            if(deptID == "STOR")
            {
                return true;
            }
            DeptCollectionDetail deptCP = db.DeptCollectionDetails.Where(x => x.departmentId == deptID).First();
            DateTime time = DateTime.Now;
            if (deptCP.authorisedPerson is null || deptCP.validDateEnd < time.Date)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}