using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ADProjGp2_LogicUStationeryStore.Models;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using System.Web.Http.Results;

namespace ADProjGp2_LogicUStationeryStore.APIControllers
{
    public class DeptCollectionDetailController : ApiController
    {
        private SSISEntities db = new SSISEntities();
        
        [HttpGet]
        public JsonResult<DeptCollectionDetail> GetDeptCollectionDetailById(string id)
        {
            return Json(db.DeptCollectionDetails.Where(x => x.departmentId == id).SingleOrDefault());
        }

        [HttpPut]
        public JsonResult<string> SetAuthorisePerson (string authperson, string headID, string deptID, string startdate, string enddate, string username, string password)
        {
            CommonBusinessLogic commonBusinessLogic = new CommonBusinessLogic();
            return Json(commonBusinessLogic.SetAuthorisedPersonWebAPI(authperson, headID, deptID, startdate, enddate, username, password));
        }

        [HttpPut]
        public JsonResult<string> CancelAuthorisePerson(string headID, string deptID, string username, string password)
        {
            CommonBusinessLogic commonBusinessLogic = new CommonBusinessLogic();
            return Json(commonBusinessLogic.CancelAuthorisedPersonWebAPI(headID, deptID, username, password));
        }

        [HttpPut]
        public JsonResult<string> SetRepresentative(string newrepid, string deptID, string headID, string username, string password)
        {
            CommonBusinessLogic commonBusinessLogic = new CommonBusinessLogic();
            return Json(commonBusinessLogic.SetRepresentativeWebAPI(newrepid, deptID, headID, username, password));
        }

        [HttpPut]
        public JsonResult<string> CancelRepresentative(string deptID, string headID, string username, string password)
        {
            CommonBusinessLogic commonBusinessLogic = new CommonBusinessLogic();
            return Json(commonBusinessLogic.CancelRepresentativeWebAPI(deptID, headID, username, password));
        }

    }
}
