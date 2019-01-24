using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.APIControllers
{
    public class AdjustmentController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        [HttpGet]
        public JsonResult<List<Adjustment>> GetAdjustments()
        {
            return Json(db.Adjustments.ToList());
        }

        [HttpPost]
        public HttpResponseMessage PostAdjustment(Adjustment adjustment)
        {
            db.Adjustments.Add(adjustment);
            db.SaveChanges();
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }
    }
}
