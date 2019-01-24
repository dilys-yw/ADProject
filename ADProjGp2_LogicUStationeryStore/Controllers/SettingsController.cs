using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class SettingsController : Controller
    {
        private SSISEntities db = new SSISEntities();
        private CommonBusinessLogic commonBizLogic = new CommonBusinessLogic();
        
        public ActionResult SetCollectionPoint()
        {
            if (Session["Role"] is null || !Session["Role"].ToString().Contains("rep"))
            {
                return RedirectToAction("Login", "Home");
            }
            Dictionary<string, string> tempdata = new Dictionary<string, string>();
            tempdata = commonBizLogic.RetrieveCurrentCollectionInfo(Session["DepartmentID"].ToString(), tempdata);
            ViewBag.CollectPoint = tempdata["CollectPoint"];
            ViewBag.CollectTime = tempdata["CollectTime"];
            List<string> collectionPtList = commonBizLogic.GetLocationsNTimeList();
            return View(collectionPtList);
        }

        [HttpPost]
        public async Task<ActionResult> SetCollectionPoint(FormCollection frm)
        {
            string selection = frm["Point"];
            await commonBizLogic.SetCurrentCollectionInfo(Session["DepartmentID"].ToString(),selection);
            Dictionary<string, string> tempdata = new Dictionary<string, string>();
            tempdata = commonBizLogic.RetrieveCurrentCollectionInfo(Session["DepartmentID"].ToString(), tempdata);
            ViewBag.CollectPoint = tempdata["CollectPoint"];
            ViewBag.CollectTime = tempdata["CollectTime"];
            List<string> collectionPtList = commonBizLogic.GetLocationsNTimeList();
            return View(collectionPtList);
        }

        public async Task<ActionResult> CancelAuthP()
        {
            if (Session["Role"] is null || Session["Role"].ToString() != "head")
            {
                return RedirectToAction("Login", "Home");
            }
            await commonBizLogic.CancelAuthorisedPerson(Session);
            return RedirectToAction("AuthoriseNRep");
        }

        public async Task<ActionResult> CancelRep()
        {
            if (Session["Role"] is null || Session["Role"].ToString() != "head")
            {
                return RedirectToAction("Login", "Home");
            }
            string headID = Session["EmployeeID"].ToString();
            string deptID = Session["DepartmentID"].ToString();
            await commonBizLogic.CancelRepresentative(deptID, Session);
            return RedirectToAction("AuthoriseNRep");
        }

        public ActionResult AuthorisePerson(string authperson, string begindatep, string enddatep)
        {
            if (Session["Role"] is null || Session["Role"].ToString() != "head")
            {
                return RedirectToAction("Login", "Home");
            }
            bool authoriseStatus = commonBizLogic.SetAuthorisedPerson(authperson, begindatep, enddatep, Session);
            Session["AuthPStatus"] = authoriseStatus;
            Session["RepStatus"] = null;
            return RedirectToAction("AuthoriseNRep", authoriseStatus);
        }

        public ActionResult SetRepresentative(string repname)
        {
            if (Session["Role"] is null || Session["Role"].ToString() != "head")
            {
                return RedirectToAction("Login", "Home");
            }
            bool repStatus = commonBizLogic.SetRepresentative(repname, Session);
            Session["RepStatus"] = repStatus;
            Session["AuthPStatus"] = null;
            return RedirectToAction("AuthoriseNRep");
        }


        public ActionResult AuthoriseNRep()
        {
            if (Session["Role"] is null || Session["Role"].ToString() != "head")
            {
                return RedirectToAction("Login", "Home");
            }
            string deptID = Session["DepartmentID"].ToString();
            string headID = Session["EmployeeID"].ToString();
            Dictionary<string, string> departmentEmployeeList;
            departmentEmployeeList = (Dictionary<string, string>) Session["EmployeeList"];
            ViewBag.authorisedPerson = commonBizLogic.GetAuthPersonRep(deptID, headID);
            return View(departmentEmployeeList);
        }


        [HttpPost]
        public ActionResult AuthoriseNRep(string beginDate, string endDate)
        {
            if (Session["Role"] is null || Session["Role"].ToString() != "head")
            {
                return RedirectToAction("Login", "Home");
            }
            string deptID = Session["DepartmentID"].ToString();
            string headID = Session["EmployeeID"].ToString();
            Dictionary<string, string> departmentEmployeeList;
            departmentEmployeeList = (Dictionary<string, string>)Session["EmployeeList"];
            ViewBag.authorisedPerson = commonBizLogic.GetAuthPersonRep(deptID, headID);
            return View(departmentEmployeeList);
        }

    }
}