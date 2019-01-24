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
    public class RetrievalController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        [HttpGet]
        public JsonResult<List<Retrieval>> GetRetrieval()
        {
            return Json(db.Retrievals.Where(c => c.status != "Cancelled").OrderBy(y => y.retrievalCreationDate).ToList());
        }

        
        [HttpPut]
        public HttpResponseMessage UpdateRetrieval(Retrieval retrieval)
        {
            var rev = db.Retrievals.Where(c => c.retrievalId == retrieval.retrievalId).FirstOrDefault();

            rev.clerkId = retrieval.clerkId;
            rev.retrievalCreationDate = retrieval.retrievalCreationDate;
            rev.status = retrieval.status;

            db.SaveChanges();

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;

        }

        //[HttpDelete]
        //public HttpResponseMessage RemoveRetrieval(string id)
        //{
        //    var details = db.Retrievals.SingleOrDefault(c => c.retrievalId == id);
        //    db.Retrievals.Remove(details);
        //    db.SaveChanges();

        //    var resp = new HttpResponseMessage(HttpStatusCode.OK);
        //    return resp;
        //}
    }
}
