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
    public class RetrievalDetailsController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        //[HttpGet]
        //public IEnumerable<RequisitionDetail> GetRetrievalListOfItem(string itemId, string retrievalId)
        //{
        //    return entity.RequisitionDetails.Where(x => (x.Requisition.retrievalId == retrievalId) && (x.itemId == itemId)).ToList();
        //}
        //[HttpGet]
        //public List<List<string>> GetRetrievalListOfItem(string itemId, string retrievalId)
        //{
        //    List<List<string>> retrievalLists = new List<List<string>>();
        //    var q = entity.RequisitionDetails.Where(x => (x.itemId == itemId) && (x.Requisition.retrievalId == retrievalId)).ToList();
        //    for (int i = 0; i < q.Count; i++)
        //    {
        //        List<string> result = new List<string>();
        //        result.Add(retrievalId);
        //        result.Add(itemId);
        //        result.Add(q[i].requestQty.ToString());
        //        result.Add(q[i].retrieveQty.ToString());
        //        result.Add(q[i].adjustQty.ToString());
        //        retrievalLists.Add(result);
        //    }
        //    return retrievalLists;
        //}

        [HttpGet]
        public JsonResult<List<Dictionary<string, string>>> GetRetrievalListOfItem(string retrievalId)
        {
            List<Dictionary<string, string>> retrievals = new List<Dictionary<string, string>>();
            List<string> items = db.RequisitionDetails.Where(x => x.Requisition.retrievalId == retrievalId).Select(y => y.itemId).Distinct().ToList();
            for (int i = 0; i < items.Count; i++)
            {
                string itemOne = items[i];
                var q = db.RequisitionDetails.Where(x => (x.itemId == itemOne) && (x.Requisition.retrievalId == retrievalId)).ToList();
                int reqQty = 0;
                int retQty = 0;
                int adjQty = 0;
                for (int j = 0; j < q.Count; j++)
                {
                    reqQty = reqQty + q[j].requestQty;
                    if (q[j].retrieveQty == null)
                    {
                        q[j].retrieveQty = 0;
                        retQty = retQty + (int)q[j].retrieveQty;
                    }
                    else
                    {
                        retQty = retQty + (int)q[j].retrieveQty;
                    }
                    if (q[j].adjustQty == null)
                    {
                        q[j].adjustQty = 0;
                        adjQty = adjQty + (int)q[j].adjustQty;
                    }
                    else
                    {
                        adjQty = adjQty + (int)q[j].adjustQty;
                    }
                }
                Dictionary<string, string> retrieval = new Dictionary<string, string>();
                retrieval.Add("retrievalId", retrievalId);
                retrieval.Add("itemId", itemOne);
                retrieval.Add("requestQty", reqQty.ToString());
                retrieval.Add("retrieveQty", retQty.ToString());
                retrieval.Add("adjustQty", adjQty.ToString());
                retrievals.Add(retrieval);
            }
            return Json(retrievals);
        }



        [HttpPut]
        public HttpResponseMessage allocateAdjustQty(string itemId, string retrievalId, string totalQty)
        {
            int total = Int32.Parse(totalQty);
            int temp = total;
            List<RequisitionDetail> rdList = db.RequisitionDetails.Where(x => (x.Requisition.retrievalId == retrievalId) && (x.itemId == itemId)).OrderBy(x => x.Requisition.approvalDate).ToList();
            for (int i = 0; i < rdList.Count; i++)
            {
                if (rdList[i].requestQty < temp)
                {
                    rdList[i].adjustQty = rdList[i].requestQty;
                }
                else
                {
                    rdList[i].adjustQty = temp;
                }
                if (temp - rdList[i].requestQty <= 0)
                {
                    temp = 0;
                }
                else
                {
                    temp = temp - rdList[i].requestQty;
                }
            }

            for (int i = 0; i < rdList.Count; i++)
            {
                string reqId = rdList[i].requisitionId;
                db.RequisitionDetails.Where(x => (x.itemId == itemId) && (x.requisitionId == reqId)).First().retrieveQty = rdList[i].adjustQty;
                db.SaveChanges();
            }
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }
    
     }
}
