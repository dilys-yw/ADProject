using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class DeliveryOrderBusinessLogic : Controller
    {
        SSISEntities db = new SSISEntities();
        public DeliverOrderViewModel DeliveryOrderEntryPreparation(string PONumber)
        {
            PurchaseOrder purchaseOrder =  db.PurchaseOrders.Find(PONumber);
            List<PurchaseOrderDetail> purchaseOrderDetails = db.PurchaseOrderDetails.Where(x => x.poId == PONumber).ToList<PurchaseOrderDetail>(); 
            DeliverOrderViewModel dovm = new DeliverOrderViewModel();
            dovm.dovmdList = new List<DeliverOrderViewModelDetail>();
            int totaldeliveredQty = 0;
            foreach (PurchaseOrderDetail pod in purchaseOrderDetails)
            {
                DeliverOrderViewModelDetail dovmd = new DeliverOrderViewModelDetail();
                // Marking fulfilled item for tracking
                totaldeliveredQty = CheckItemTotalDeliveredQty(pod.itemId, pod.poId);
                if(totaldeliveredQty >= pod.quantity)
                {
                    dovmd.remark = "Fulfilled";
                    dovmd.IsMatched = true;
                }
                else
                {
                    dovmd.remark = null;
                    dovmd.IsMatched = false;
                }
                dovmd.podID = pod.id;

                dovmd.itemNumber = pod.itemId;
                dovmd.itemDescription = db.Catalogues.Find(pod.itemId).description;
                dovmd.itemUOM = db.Catalogues.Find(pod.itemId).unitOfMeasure;
                if (dovmd.itemUOM != "Dozen" || dovmd.itemUOM != "Each")
                {
                    dovmd.itemUOM = dovmd.itemUOM + " of " + db.Catalogues.Find(pod.itemId).quantityInUnit;
                }
                dovmd.purchaseQty = pod.quantity;
                dovmd.deliverQty = pod.quantity;
                dovm.dovmdList.Add(dovmd);
            }
            dovm.poNumber = PONumber;
            dovm.orderRemark = purchaseOrder.remark;
            dovm.supplierID = purchaseOrder.supplierId;
            dovm.clerkID = purchaseOrder.storeClerk;
            dovm.orderDate = purchaseOrder.orderDate;

            return dovm;
        }

        public DeliverOrderViewModel DeliverOrderMatching(DeliverOrderViewModel dovm)
        {             
            foreach (DeliverOrderViewModelDetail x in dovm.dovmdList)
            {
                if (x.purchaseQty == x.deliverQty)
                {
                    x.IsMatched = true;
                    x.remark = "Complete";
                }
                else if (x.purchaseQty < x.deliverQty)
                {
                    x.IsMatched = true;
                    x.remark = "Over";
                }
                else
                {
                    x.IsMatched = false;
                    x.remark = "Shortage";
                }
            }
            if (dovm.dovmdList.Where(x => x.IsMatched == false).Any())
            {
                if (dovm.status == "Pending")
                {
                    dovm.status = "Partial Fulfillment by Deliver Order :" + dovm.doNumber;
                }
                else
                {
                    dovm.status = dovm.status + " , Partial Fulfillment by Deliver Order :" + dovm.doNumber;
                }
                if (dovm.deliverRemark != null)
                {
                    dovm.deliverRemark = dovm.deliverRemark + " , Partial Fulfillment by Deliver Order :" + dovm.doNumber;
                }
                else
                {
                    dovm.deliverRemark = "Partial Fulfillment by Deliver Order :" + dovm.doNumber;
                }
            }
            else
            {
                if (dovm.status == "Pending")
                {
                    dovm.status = "Fulfilled by Deliver Order :" + dovm.doNumber;
                }
                else
                {
                    dovm.status = dovm.status + " , Fulfilled by Deliver Order :" + dovm.doNumber;
                }
                
                if (dovm.deliverRemark != null)
                {
                    dovm.deliverRemark = dovm.deliverRemark + " , Completed by Deliver Order :" + dovm.doNumber;
                }
                else
                {
                    dovm.deliverRemark = "Completed by Deliver Order :" + dovm.doNumber;
                }
            }
            return dovm;
        }

        public void DeliveryConfirmation (DeliverOrderViewModel dovm)
        {
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(dovm.poNumber);
            purchaseOrder.remark = dovm.status;
            DeliveryOrder deliveryOrder = new DeliveryOrder();
            deliveryOrder.doId = dovm.doNumber;
            deliveryOrder.poId = dovm.poNumber;
            deliveryOrder.deliveryDate = dovm.doDate;
            deliveryOrder.remark = dovm.deliverRemark;
            db.DeliveryOrders.Add(deliveryOrder);
            foreach(DeliverOrderViewModelDetail x in dovm.dovmdList)
            {
                DeliveryOrderDetail dod = new DeliveryOrderDetail();
                dod.doId = dovm.doNumber;
                dod.itemId = x.itemNumber;
                dod.quantity = x.deliverQty;
                db.DeliveryOrderDetails.Add(dod);
            }
            db.SaveChangesAsync();
        }

        public int CheckItemTotalDeliveredQty(string itemID, string POID)
        {
            List<DeliveryOrder> deliveryOrders = db.DeliveryOrders.Where(x => x.poId == POID).ToList();
            int totalDeliveredQty = 0;
            if (!deliveryOrders.Any())
            {
                return totalDeliveredQty;
            }
            List<string> deliverOrderNumbers = deliveryOrders.Select(x => x.doId).ToList();

            //Establish the total deliveredQty (if any)            
            foreach (string doID in deliverOrderNumbers)
            {
                List<DeliveryOrderDetail> deliveryOrderDetails = db.DeliveryOrderDetails.Where(x => x.doId == doID).ToList<DeliveryOrderDetail>();
                totalDeliveredQty += deliveryOrderDetails.Where(x => x.itemId == itemID).Select(y => y.quantity).Sum();
            }
            return totalDeliveredQty;
        }


    }
}