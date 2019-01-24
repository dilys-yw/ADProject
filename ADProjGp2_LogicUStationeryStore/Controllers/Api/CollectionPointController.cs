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
    public class CollectionPointController : ApiController
    {
        private SSISEntities db = new SSISEntities();
        
        [HttpGet]
        public JsonResult<List<CollectionPoint>> GetCollectionPoint ()
        {
            return Json(db.CollectionPoints.ToList());
        }
    }
}
