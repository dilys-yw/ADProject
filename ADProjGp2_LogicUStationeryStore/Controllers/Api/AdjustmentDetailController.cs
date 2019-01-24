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
    public class AdjustmentDetailController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        [HttpGet]
        public JsonResult<List<AdjustmentDetail>> GetAdjustments()
        {
            return Json(db.AdjustmentDetails.ToList());
        }

        [HttpPost]
        public HttpResponseMessage PostAdjustmentDetail (IEnumerable<AdjustmentDetail> ad)
        {
            foreach (AdjustmentDetail adt in ad)
            {
                db.AdjustmentDetails.Add(adt);
                db.SaveChanges();
            }
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }
    }
}

