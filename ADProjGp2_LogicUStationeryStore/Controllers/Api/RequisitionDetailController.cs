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
    public class RequisitionDetailController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        //URL ??????//
        [HttpGet]
        public JsonResult<List<String>> GetItemList(string retrievalId)
        {
            return Json(db.RequisitionDetails.Where(c => c.Requisition.retrievalId == retrievalId).Select(x => x.itemId).Distinct().ToList());
        }

        [HttpGet]
        public JsonResult<List<RequisitionDetail>> GetRequisitionDetailById(string id)
        {
            return Json(db.RequisitionDetails.Where(c => c.requisitionId == id).ToList());
        }

        [HttpPost]
        public HttpResponseMessage PostRequisitionDetail ([FromBody] RequisitionDetail req)
        {
            db.RequisitionDetails.Add(req);
            db.SaveChanges();
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }
        
    }
}
