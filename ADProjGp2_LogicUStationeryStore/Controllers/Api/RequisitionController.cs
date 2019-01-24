using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.APIControllers
{
    public class RequisitionController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        //GET api/Requisition
        [HttpGet]
        public JsonResult<List<Requisition>> GetRequistion()
        {
            return Json(db.Requisitions.ToList());
        }

        [HttpGet]
        public JsonResult<List<Requisition>> GetRequisitionsById(string id)
        {
            return Json(db.Requisitions.Where(c => c.employee == id).ToList());
        }

        [HttpGet]
        public JsonResult<Requisition> GetRequisitionByReq(string req)
        {
            return Json(db.Requisitions.Where(c => c.requisitionId == req).FirstOrDefault());
        }

        //Get Requisition by empId & status
        [HttpGet]
        public JsonResult<List<Requisition>> GetRequisitionByIdandStatus(string empId, string status)
        {
            return Json(db.Requisitions.Where(c => c.employee == empId && c.status.ToLower() == status.ToLower()).OrderBy(c => c.approvalDate).ToList());
        }

        //Get Requisition by depId (For head)
        [HttpGet]
        public JsonResult<List<Requisition>> GetRequisitionByDepId(string depId)
        {
            return Json(db.Requisitions.Where(c => c.departmentId == depId).ToList());
        }

        //Get Requisition by depId & status
        [HttpGet]
        public JsonResult<List<Requisition>> GetRequisitionByDepIdandStatus(string depId, string status)
        {
            return Json(db.Requisitions.Where(c => c.departmentId == depId && c.status == status).ToList());
        }
        
        //POST api/Requisition
        [HttpPost]
        public HttpResponseMessage CreateRequisition([FromBody] Requisition requisition)
        {
            db.Requisitions.Add(requisition);
            db.SaveChanges();
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }
        
        [HttpPut]
        public HttpResponseMessage EditRequisition(Requisition requisition)
        {
            Requisition details = db.Requisitions.SingleOrDefault(c => c.requisitionId == requisition.requisitionId);

            details.status = requisition.status;
            details.approvalDate = requisition.approvalDate;
            details.approvalPerson = requisition.approvalPerson;
            details.remark = requisition.remark == null ? null : requisition.remark;

            db.SaveChanges();

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }

        //[HttpDelete]
        //public HttpResponseMessage RemoveRequisition(string id)
        //{
        //    var details = db.Requisitions.SingleOrDefault(c => c.requisitionId == id);
        //    db.Requisitions.Remove(details);
        //    db.SaveChanges();
            
        //    var resp = new HttpResponseMessage(HttpStatusCode.OK);
        //    return resp;
        //}
        
    }
}

