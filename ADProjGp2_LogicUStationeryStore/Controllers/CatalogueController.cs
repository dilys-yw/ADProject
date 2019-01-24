using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class CatalogueController : Controller
    {
        private SSISEntities db = new SSISEntities();


        public ActionResult Stocktake(string empID)
        {
            if (empID == null)
            {
                empID = Session["EmployeeID"].ToString();
            }
            CommonBusinessLogic commonBizLogic = new CommonBusinessLogic();
            List<StockTakeModel> model = commonBizLogic.GenerateStocktake(empID);
            return View(model);
        }

        [HttpPost]
        public ActionResult StockTakeAdjust(List<StockTakeModel> stmList)
        {
            AdjustmentBusinessLogic adjBizLogic = new AdjustmentBusinessLogic();
            Dictionary<string, int> adjItemList = adjBizLogic.AdjItemsConversion(stmList);
            string adjID = adjBizLogic.CreateStockTakeAdjustmentVoucher(Session, adjItemList);
            return RedirectToAction("AdjustmentDetail","Adjustment",new { adjustmentID = adjID });
        }

        // GET: Catalogue
        public ActionResult Index()
        {
            var catalogues = db.Catalogues.Include(c => c.Supplier).Include(c => c.Supplier1).Include(c => c.Supplier2).Include(c => c.Inventory);
            return View(catalogues.ToList());
        }

        // GET: Catalogue/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogue catalogue = db.Catalogues.Find(id);
            if (catalogue == null)
            {
                return HttpNotFound();
            }
            return View(catalogue);
        }

        // GET: Catalogue/Create
        public ActionResult Create()
        {
            ViewBag.firstSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber");
            ViewBag.secondSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber");
            ViewBag.thirdSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber");
            ViewBag.itemId = new SelectList(db.Inventories, "itemId", "itemId");
            return View();
        }

        // POST: Catalogue/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "itemId,category,description,unitOfMeasure,reorderQuantity,reorderLevel,firstSupplier,secondSupplier,thirdSupplier,bin,quantityInUnit")] Catalogue catalogue)
        {
            if (ModelState.IsValid)
            {
                db.Catalogues.Add(catalogue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.firstSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.firstSupplier);
            ViewBag.secondSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.secondSupplier);
            ViewBag.thirdSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.thirdSupplier);
            ViewBag.itemId = new SelectList(db.Inventories, "itemId", "itemId", catalogue.itemId);
            return View(catalogue);
        }

        // GET: Catalogue/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogue catalogue = db.Catalogues.Find(id);
            if (catalogue == null)
            {
                return HttpNotFound();
            }
            ViewBag.firstSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.firstSupplier);
            ViewBag.secondSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.secondSupplier);
            ViewBag.thirdSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.thirdSupplier);
            ViewBag.itemId = new SelectList(db.Inventories, "itemId", "itemId", catalogue.itemId);
            return View(catalogue);
        }

        // POST: Catalogue/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "itemId,category,description,unitOfMeasure,reorderQuantity,reorderLevel,firstSupplier,secondSupplier,thirdSupplier,bin,quantityInUnit")] Catalogue catalogue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catalogue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.firstSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.firstSupplier);
            ViewBag.secondSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.secondSupplier);
            ViewBag.thirdSupplier = new SelectList(db.Suppliers, "supplierId", "gstNumber", catalogue.thirdSupplier);
            ViewBag.itemId = new SelectList(db.Inventories, "itemId", "itemId", catalogue.itemId);
            return View(catalogue);
        }

        // GET: Catalogue/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogue catalogue = db.Catalogues.Find(id);
            if (catalogue == null)
            {
                return HttpNotFound();
            }
            return View(catalogue);
        }

        // POST: Catalogue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Catalogue catalogue = db.Catalogues.Find(id);
            db.Catalogues.Remove(catalogue);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
