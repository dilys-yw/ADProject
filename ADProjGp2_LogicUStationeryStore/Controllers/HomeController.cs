using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ADProjGp2_LogicUStationeryStore.Models;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using System.Web.Security;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class HomeController : Controller
    {
        CommonBusinessLogic commonBizLogic = new CommonBusinessLogic();
        public ActionResult Login()
        {
            return View(new LoginObjectModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginObjectModel model)
        {
            if (ModelState.IsValid)
            {
                model = commonBizLogic.ValidateUser(model);
                //Login fail
                if (model is null)
                {
                    ViewBag.NotAuthorised = true;
                    return View();
                }
                Dictionary<string, string> empList = commonBizLogic.GetEmployeeList(model.userName, model.password, model.departmentId);
                commonBizLogic.PrepareSession(model, empList, Session);
                //currently testing using requisition history page
                return RedirectToAction("Welcome", "Home");
            }
            // Model State failed (basic validation), redisplay form  
            return View(model);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); // it will clear the session at the end of request
            return RedirectToAction("Login", "Home");
        }

        public ActionResult Welcome()
        {
            if(Session["Role"].ToString() == "clerk")
            {
                return RedirectToAction("ClerkWelcome");
            }
            else if (Session["Role"].ToString() == "supervisor")
            {
                return RedirectToAction("SupervisorWelcome");
            }
            else if (Session["Role"].ToString() == "manager")
            {
                return RedirectToAction("ManagerWelcome");
            }
            return View();
        }

        public ActionResult ClerkWelcome()
        {
            ClerkWelcomePageModel model = commonBizLogic.GetClerkWelcomePageModel(Session);
            return View(model);
        }
        public ActionResult SupervisorWelcome()
        {
            SupervisorWelcomePageModel model = commonBizLogic.GetSupervisorWelcomePageModel(Session);
            return View(model);
        }
        public ActionResult ManagerWelcome()
        {
            ManagerWelcomePageModel model = commonBizLogic.GetManagerWelcomePageModel(Session);
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                model = commonBizLogic.ChangePassword(model);
                //Login fail
                if (model.replymsg != "Password change success")
                { 
                    return View(model);
                }
                //currently testing using requisition history page
                return RedirectToAction("Login", "Home");
            }
            // Model State failed (basic validation), redisplay form  
            return View(model);
        }

        public ActionResult CPmap()
        {
            return View();
        }
    }
}