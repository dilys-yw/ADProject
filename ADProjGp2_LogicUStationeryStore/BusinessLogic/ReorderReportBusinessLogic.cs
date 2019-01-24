using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class PODetailsModel
    {
        public string PoID { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderQty { get; set; }
        public string SupplierID { get; set; }
        public string Category { get; set; }
    }
    public class ReorderReportBusinessLogic
    {
        SSISEntities entity = new SSISEntities();

        public IList<SelectListItem> DisplaySuppliers()
        {

            List<string> suppliers = entity.Suppliers.Select(c => c.supplierId).ToList();
            IList<SelectListItem> supps = new List<SelectListItem>();


            foreach (string sup in suppliers)
            {
                supps.Add(new SelectListItem
                {
                    Text = sup,
                    Value = sup,
                });
            }
            return supps;
        }

        public int[] GetItems()
        {
            var totalReorder = (from po in entity.PurchaseOrders
                                    join pod in entity.PurchaseOrderDetails
                                    on po.poId equals pod.poId
                                    where po.orderDate.Year >= DateTime.Today.Year - 2
                                    select new PODetailsModel
                                    {
                                        PoID = po.poId,
                                        OrderDate = po.orderDate,
                                        OrderQty = pod.quantity,

                                    }).ToList();


            int currentYearQuantity = 0;
            int lastYearQuantity = 0;
            int previousYearQuantity = 0;

            foreach (var po in totalReorder)
            {
                if (po.OrderDate.Year == DateTime.Today.Year - 2)
                {
                    previousYearQuantity += po.OrderQty;
                }
                else if (po.OrderDate.Year == DateTime.Today.Year - 1)
                {
                    lastYearQuantity += po.OrderQty;
                }
                else if (po.OrderDate.Year == DateTime.Today.Year)
                {
                    currentYearQuantity += po.OrderQty;
                }
            }

            int[] data = new int[3] { previousYearQuantity, lastYearQuantity, currentYearQuantity };

            return data;
        }

        //same
        public int[] GetItemsMonthly(string year)
        {

            var details = (from po in entity.PurchaseOrders
                           join pod in entity.PurchaseOrderDetails
                           on po.poId equals pod.poId
                           where po.orderDate.Year.ToString() == year
                           select new PODetailsModel
                           {
                               PoID = po.poId,
                               OrderDate = po.orderDate,
                               OrderQty = pod.quantity,
                               SupplierID = po.supplierId,

                           }).ToList();


            int[] reorderQuantity = new int[12];

            foreach (var po in details)
            {
                int month = po.OrderDate.Month;

                reorderQuantity[month - 1] += po.OrderQty;

            }

            return reorderQuantity;
        }

        public Dictionary<string, int> GetTotalBySupplier(string supName, List<string> years)
        {

            var details = (from po in entity.PurchaseOrders
                           join pod in entity.PurchaseOrderDetails
                           on po.poId equals pod.poId
                           where po.supplierId == supName
                           select new PODetailsModel
                           {
                               PoID = po.poId,
                               OrderDate = po.orderDate,
                               OrderQty = pod.quantity,
                               SupplierID = po.supplierId,

                           }).ToList();


            Dictionary<string, int> suppQty = new Dictionary<string, int>();

            foreach (string year in years)
            {
                int totalOrder = 0;
                for (int i = 0; i < details.Count; i++)
                {
                    if (details[i].OrderDate.Year.ToString() == year)
                    {
                        totalOrder += details[i].OrderQty;
                    }
                }
                suppQty.Add(year, totalOrder);
            }

            return suppQty;
        }

        public Dictionary<string, int> GetTotalBySupplier(string supName, string catg, List<string> years)
        {

            var details = (from po in entity.PurchaseOrders
                           join pod in entity.PurchaseOrderDetails
                           on po.poId equals pod.poId
                           join item in entity.Catalogues
                           on pod.itemId equals item.itemId
                           where (po.supplierId == supName
                           && item.category == catg)
                           select new PODetailsModel
                           {
                               PoID = po.poId,
                               OrderDate = po.orderDate,
                               OrderQty = pod.quantity,
                               SupplierID = po.supplierId,
                               Category = item.category,

                           }).ToList();


            Dictionary<string, int> suppQty = new Dictionary<string, int>();

            foreach (string year in years)
            {
                int totalOrder = 0;
                for (int i = 0; i < details.Count; i++)
                {
                    if (details[i].OrderDate.Year.ToString() == year)
                    {
                        totalOrder += details[i].OrderQty;
                    }
                }
                suppQty.Add(year, totalOrder);
            }

            return suppQty;
        }

        public Dictionary<int, int> GetTotalBySuppAndYear(string suppName, string year, List<int> selectedMonths)
        {

            var details = (from po in entity.PurchaseOrders
                           join pod in entity.PurchaseOrderDetails
                           on po.poId equals pod.poId
                           where (po.supplierId == suppName
                           && po.orderDate.Year.ToString() == year)
                           select new PODetailsModel
                           {
                               PoID = po.poId,
                               OrderDate = po.orderDate,
                               OrderQty = pod.quantity,
                               SupplierID = po.supplierId,

                           }).ToList();


            Dictionary<int, int> suppQty = new Dictionary<int, int>();

            if (selectedMonths != null)
            {

                foreach (int month in selectedMonths)
                {
                    int totalOrder = 0;
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].OrderDate.Month == month)
                        {
                            totalOrder += details[i].OrderQty;
                        }
                    }
                    suppQty.Add(month, totalOrder);

                }
            }

            return suppQty;
        }

        public Dictionary<int, int> GetTotalBySuppAndYear(string suppName, string year, List<int> selectedMonths, string item)
        {

            var details = (from po in entity.PurchaseOrders
                           join pod in entity.PurchaseOrderDetails
                           on po.poId equals pod.poId
                           join cat in entity.Catalogues
                           on pod.itemId equals cat.itemId
                           where (po.supplierId == suppName
                           && po.orderDate.Year.ToString() == year
                           && cat.category == item)
                           select new PODetailsModel
                           {
                               PoID = po.poId,
                               OrderDate = po.orderDate,
                               OrderQty = pod.quantity,
                               SupplierID = po.supplierId,
                               Category = cat.category,

                           }).ToList();


            Dictionary<int, int> orderQty = new Dictionary<int, int>();
            if (selectedMonths != null)
            {
                foreach (int month in selectedMonths)
                {
                    int totalOrder = 0;
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].OrderDate.Month == month)
                        {
                            totalOrder += details[i].OrderQty;
                        }
                    }
                    orderQty.Add(month, totalOrder);
                }
            }

            return orderQty;
        }

    }
}