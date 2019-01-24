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
    public class CatalogueController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        //GET api/Requisition
        [HttpGet]
        public JsonResult<List<Catalogue>> GetCatalogue()
        {
            return Json(db.Catalogues.ToList());
        }

        [HttpGet]
        public JsonResult<Catalogue> GetCatalogue(string id)
        {
            return Json(db.Catalogues.SingleOrDefault(c => c.itemId == id));
        }

        [HttpGet]
        public JsonResult<List<Catalogue>> GetCatalogueByCat (string category)
        {
            return Json(db.Catalogues.Where(c => c.category.ToLower() == category.ToLower()).ToList());
        }
    }
}
