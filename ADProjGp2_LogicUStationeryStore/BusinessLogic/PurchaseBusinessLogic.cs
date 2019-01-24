using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;
using MathNet.Numerics;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class PurchaseBusinessLogic : Controller
    {
        SSISEntities db = new SSISEntities();

        //Upon new PO creation - this is the main method
        public List<SuggestionViewModel> GetSuggestionList()
        {
            List<string> itemList = db.Catalogues.Select(x => x.itemId).ToList();
            List<SuggestionViewModel> listSuggest = new List<SuggestionViewModel>();

            //Iterate catalogue and retrieve items that fulfill re-order conditions
            foreach (string item in itemList)
            {
                SuggestionViewModel orderItem = AdviceOnOrderQty(item);
                if (orderItem != null)
                {
                    listSuggest.Add(orderItem);
                }
            }
            return listSuggest;
        }
        
        public SuggestionViewModel AdviceOnOrderQty(string itemNo)
        {
            string itemName = db.Catalogues.Where(x => x.itemId == itemNo).Select(x => x.description).First();
            int qtyToOrder = 0;
            SuggestionViewModel itemToOrder = new SuggestionViewModel();
            itemToOrder.ItemId = itemNo;
            itemToOrder.Description = itemName;

            // Step 1 - item in special list
            // Item will only appear as special because of qty not fulfilled, qty is -ve in inventory sense and thus satisfy a reorder.
            itemToOrder = AdviceOnSpecialRequest(itemToOrder);             

            // Step 2 - Establish Reorder Level and Qty
            // By design - reorder level and qty represents user decision, and therefore will establish the hard boundaries for minimum reorder
            // if reorder level is reached, the stock will be replenished at reorder qty as long as the adviceQty is lower than reorderQty
            int theReorderLvl = db.Catalogues.Where(x => x.itemId == itemNo).Select(x => x.reorderLevel).First();
            int theReorderQty = db.Catalogues.Where(x => x.itemId == itemNo).Select(x => x.reorderQuantity).First();
            int qtyInStore = db.Inventories.Where(x => x.itemId == itemNo).Select(x => x.storeQuantity).First();

            // Step 3 Give suggestion based on history analysis
            // Calculate adviceByLR using Linear Regression
            // The regression advice will only be reflected on meeting increasing demands. (i.e. suggestion to order more than ReorderQty to meet demand)
            // If there is a pettern of decreasing demand, user set reorderQty is still used by default. There will be remarks though.
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month - 1;
            if(month == 0)
            {
                year = year - 1;
                month = 12;
            }
            int reqQty = db.RequisitionDetails.Where(x => (x.itemId == itemNo) && (x.Requisition.requestDate.Year == year) && (x.Requisition.requestDate.Month == month)).Select(x => x.requestQty).FirstOrDefault();

            if (qtyInStore <= theReorderLvl)
            {
                //perform regression only if reordering level is reached (performance issue)
                int adviceByLR = QtyByAnalysis(itemNo, reqQty);
                if (theReorderQty < adviceByLR)
                {
                    qtyToOrder = qtyToOrder + adviceByLR;

                    if (itemToOrder.remark == null)
                    {
                        itemToOrder.remark = "Advised Qty : " + adviceByLR + " based on analysis (Current reorder Qty : " + theReorderQty+ ")";
                    }
                    else
                    {
                        itemToOrder.remark = itemToOrder.remark + " , Advised Qty : " + adviceByLR + " based on analysis (Current reorder Qty : " + theReorderQty + ")";
                    }
                }
                else
                {
                    qtyToOrder = qtyToOrder + theReorderQty;

                    if (itemToOrder.remark == null)
                    {
                        if (adviceByLR <= 0)
                            itemToOrder.remark = "Reorder Qty : " + theReorderQty;
                        else
                            itemToOrder.remark = "Reorder Qty : " + theReorderQty + " Note (Analysis suggested Qty : " + adviceByLR + ")";

                    }
                    else
                    {
                        if(adviceByLR <= 0)
                            itemToOrder.remark = itemToOrder.remark + " , Reorder Qty : " + theReorderQty;
                        else
                            itemToOrder.remark = itemToOrder.remark + " , Reorder Qty : " + theReorderQty + " Note (Analysis suggested Qty : " + adviceByLR + ")";
                    }
                }
            }

            // Step 4 Return null (no need for reorder) or the order object
            if (qtyToOrder == 0)
            {
                return null;
            }
            else
            {
                itemToOrder.SQuantity = itemToOrder.SQuantity + qtyToOrder;          
                return itemToOrder;
            }
        }

        public SuggestionViewModel AdviceOnSpecialRequest(SuggestionViewModel itemToOrder)
        {
            string itemNo = itemToOrder.ItemId;
            itemToOrder.SQuantity = itemToOrder.SQuantity + db.SpecialRequests.Where(x => x.itemId == itemNo && x.status == "Special").Select(x => x.requestQty).ToList().Sum();
            List<SpecialRequest> specialRequest = db.SpecialRequests.Where(x => x.itemId == itemNo && x.status == "Special").ToList<SpecialRequest>();
            if (specialRequest.Any())
            {
                foreach (SpecialRequest x in specialRequest)
                {
                    if (itemToOrder.remark == null)
                    {
                        itemToOrder.remark = "Special Qty : " + x.requestQty + " (Special Request ID : " + x.specialId + ")";
                    }
                    else
                    {
                        itemToOrder.remark = itemToOrder.remark + " , Special Qty : " + x.requestQty + " (Special Request ID : " + x.specialId + ")";
                    }
                }
            }
            return itemToOrder;
        }

        private int QtyByAnalysis(string itemId, int reqQtyLastM)
        {
            int predict = 0;
            List<int> reqList;
            List<int> orderList;
            //2 arrarys: (1) request qty; (2) order qty
            GetReqAndOrderHistory(itemId, out reqList,  out orderList);
            //early exit since there is no meaning for analysis if there are no requsitions or order data
            if(reqList.Sum() == 0 || orderList.Sum() == 0)
            {
                return predict;
            }
            double[] reqArray = new double[reqList.Count];
            double[] orderArray = new double[orderList.Count];
            for(int i = 0; i < reqList.Count; i++)
            {
                reqArray[i] = Convert.ToDouble(reqList[i]);
                orderArray[i] = Convert.ToDouble(orderList[i]);
            }
            Tuple<double, double> p = Fit.Line(reqArray, orderArray);
            double a = p.Item1;
            double b = p.Item2;
            predict = (int) Math.Ceiling(b * reqQtyLastM + a);
            return predict;
        }

        private void GetReqAndOrderHistory(string itemId, out List<int> reqList, out List<int> orderList)
        {
            // Prepare year and month arrays for iterations
            int[] monthArray = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            int endYear = DateTime.Now.Year;
            int startYear = endYear - 3; // Logic design to make regression analysis limited to 3 years of data 
            
            int[] yearArray = new int[endYear - startYear];
            yearArray[0] = startYear;
            for (int i = 1; i < yearArray.Length; i++)
            {
                yearArray[i] = yearArray[i - 1] + 1;
            }
            int dataLength = monthArray.Length * yearArray.Length + DateTime.Now.Month - 2 ;
            orderList = new List<int>();
            reqList = new List<int>();
          

            List<RequisitionDetail> tempList = db.RequisitionDetails.Where(x => (x.Requisition.approvalDate != null)&&(x.itemId== itemId)).ToList();
            // Capture history data before current year
            for (int i = 0; i < yearArray.Length; i++)
            {
                int year = yearArray[i];
                for (int j = 0; j < monthArray.Length; j++)
                {
                    int month = monthArray[j];
                    var tempOrder = db.PurchaseOrderDetails.Where(x => (x.itemId == itemId) && (x.PurchaseOrder.orderDate.Year == year) && (x.PurchaseOrder.orderDate.Month == month)).Select(x => x.quantity).ToList();
                    var tempReq = tempList.Where(x => (x.Requisition.requestDate.Year == year) && (x.Requisition.requestDate.Month == month)).Select(x => x.requestQty).ToList();
                    if (i != 0 && j != 0)
                    {
                        orderList.Add(tempOrder.Sum());
                    }
                    reqList.Add(tempReq.Sum());
                }
            }

            // Capture data in this year                
            if (DateTime.Now.Month == 1)
            {
                reqList.RemoveAt(monthArray.Length * yearArray.Length);
            }
            else
            {
                for (int i = 0; i < DateTime.Now.Month; i++)
                {
                    int yearNow = DateTime.Now.Year;
                    var tempOrder = db.PurchaseOrderDetails.Where(x => (x.itemId == itemId) && (x.PurchaseOrder.orderDate.Year == yearNow) && (x.PurchaseOrder.orderDate.Month == i)).Select(x => x.quantity).ToList();
                    var tempReq = tempList.Where(x => (x.Requisition.requestDate.Year == yearNow) && (x.Requisition.requestDate.Month == i)).Select(x => x.requestQty).ToList();
                    for (int m = reqList.Count; m < dataLength; m++)
                    {
                        reqList.Add(tempReq.Sum());
                    }
                    for (int n = orderList.Count; n < dataLength; n++)
                    {
                        orderList.Add(tempOrder.Sum());
                    }
                }
            }
        }       


        public List<PurchaseOrderViewModel> GetPurchaseOrderItem(List<SuggestionViewModel> list)
        {
            List<PurchaseOrderViewModel> itemList = new List<PurchaseOrderViewModel>();

            for (int i = 0; i < list.Count; i++)
            {
                var items = list.Select(x => x.ItemId).ToArray();

                itemList = (from c in db.Catalogues.Where(p => items.Contains(p.itemId)).
                              AsEnumerable()
                            join param in list on c.itemId equals param.ItemId
                            select new PurchaseOrderViewModel
                            {
                                ItemId = c.itemId,
                                Description = c.description,
                                SQuantity = param.SQuantity,
                                Suppliers = new List<string> { c.firstSupplier, c.secondSupplier, c.thirdSupplier },

                            }).ToList();
            }

            for (int j = 0; j < itemList.Count; j++)
            {
                itemList[j].ItemSup = new List<SelectListItem>();

                foreach (var supplier in itemList[j].Suppliers)
                {
                    itemList[j].ItemSup.Add(new SelectListItem()
                    {
                        Value = supplier,
                        Text = supplier,
                    });
                }
            }

            return itemList;

        }

        public void AddPurchaseOrder(CustomPOViewModel cpvm, HttpSessionStateBase session)
        {
            foreach (KeyValuePair<string, string> pair in cpvm.PoList)
            {
                PurchaseOrder po = new PurchaseOrder();
                {
                    po.poId = pair.Value;
                    po.supplierId = pair.Key;
                    po.storeClerk = session["EmployeeID"].ToString();
                    po.orderDate = DateTime.Today;
                    po.remark = "Pending";
                }
                db.PurchaseOrders.Add(po);
                db.SaveChanges();
            }

            for (int i = 0; i < cpvm.Pvm.Count; i++)
            {
                PurchaseOrderDetail pod = new PurchaseOrderDetail();
                {
                    //pod.id is auto generated 
                    pod.poId = cpvm.Pvm[i].PoId;
                    pod.itemId = cpvm.Pvm[i].ItemId;
                    pod.quantity = cpvm.Pvm[i].SQuantity;
                    pod.price = cpvm.Pvm[i].Price;
                }
                db.PurchaseOrderDetails.Add(pod);
                db.SaveChanges();
            }
        }

        public CustomPOViewModel ConfirmPurchaseOrder(List<PurchaseOrderViewModel> list)
        {
            HashSet<string> supType = new HashSet<string>(list.Select(x => x.SelectedSupplier));
            Dictionary<string, string> POList = new Dictionary<string, string>();

            foreach (string element in supType)
            {
                string date = DateTime.Today.ToString("ddMMyyyyhhmmss");
                POList.Add(element, element + date);

            }
            for (int i = 0; i < list.Count; i++)
            {
                string supplier = list[i].SelectedSupplier;
                string itemId = list[i].ItemId;
                list[i].Price = db.SupplierQuotations.Where(x => (x.itemId == itemId) && (x.supplierId == supplier)).Select(x => x.price).First();
                list[i].Amount = list[i].Price * list[i].SQuantity;

                foreach (KeyValuePair<string, string> pair in POList)
                {
                    if (list[i].SelectedSupplier == pair.Key)
                    {
                        list[i].PoId = pair.Value;
                    }
                }
            }

            CustomPOViewModel cpvm = new CustomPOViewModel();
            {
                cpvm.Pvm = list;
                cpvm.PoList = POList;
            }
            return cpvm;
        }

        public List<PurchaseOrder> DisplayList()
        {
            List<PurchaseOrder> ListPO = new List<PurchaseOrder>();
            ListPO = db.PurchaseOrders.ToList();

            return ListPO;
        }

        public ViewDataDictionary PreparePurchaseOrderDetails(HttpSessionStateBase session, string POID, out List<PurchaseOrderViewModel> purchaseOrderViewModels)
        {
            string empID = db.PurchaseOrders.Find(POID).storeClerk;
            string clerkName = ((Dictionary<string, string>)session["EmployeeList"])[empID];
            string clerkDetails = clerkName;
            ViewDataDictionary returnViewBag = new ViewDataDictionary();
            returnViewBag.Add("EmployeeName",clerkDetails);
            returnViewBag.Add("PONumber", POID);
            returnViewBag.Add("PODate", db.PurchaseOrders.Find(POID).orderDate);
            returnViewBag.Add("Status", db.PurchaseOrders.Find(POID).remark);

            //generate pod and cal the total order cost
            List<PurchaseOrderViewModel> pod = db.PurchaseOrderDetails.Where(x => x.poId == POID).Select
                (x => new PurchaseOrderViewModel
                {
                    PoId = x.poId,
                    ItemId = x.itemId,
                    Price = x.price,
                    SQuantity = x.quantity,
                    Amount = x.price * x.quantity,
                }).ToList();
            for (int i = 0; i < pod.Count; i++)
            {
                string item = pod[i].ItemId;
                pod[i].Description = db.Catalogues.Where(x => x.itemId == item).Select(x => x.description).FirstOrDefault();
            }
            purchaseOrderViewModels = pod;
            decimal totalAmount = pod.Select(x => x.Amount).Sum();
            returnViewBag.Add("TotalAmt", totalAmount);
            return returnViewBag;
        }
    }
}
