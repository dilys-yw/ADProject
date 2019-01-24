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
    public class InventoryController : ApiController
    {
        private SSISEntities db = new SSISEntities();

        [HttpGet]
        public JsonResult<List<Inventory>> GetAllInventories ()
        {
            return Json(db.Inventories.ToList());
        }

        [HttpGet]
        public JsonResult<Inventory> GetInventoryById (string id)
        {
            return Json(db.Inventories.Where(c => c.itemId == id).FirstOrDefault());
        }

        [HttpPut]
        public HttpResponseMessage UpdateInventory (Inventory inventory)
        {
            var inv = db.Inventories.Where(c => c.itemId == inventory.itemId).FirstOrDefault();

            inv.storeQuantity = inventory.storeQuantity;
            inv.disburseQuantity = inventory.disburseQuantity;
            inv.adjQuantity = inventory.adjQuantity;

            db.SaveChanges();

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }

    }
}
