using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    //Requisition form cart for handling of adding and removing items
    public class RequisitionFormItemCart
    {
        private List<RequisitionDetail> reqCart;
        private SSISEntities db = new SSISEntities();

        public RequisitionFormItemCart()
        {
            reqCart = new List<RequisitionDetail>();
        }

        //Determine if item is to be added to cart or edited
        public void ItemCartAddDecision(string addItem, int addQty)
        {
            bool repeatflag = false;
            int lastID = 0;
            RequisitionDetail tempitem = new RequisitionDetail();
            List<Catalogue> itemList = db.Catalogues.ToList<Catalogue>();
            foreach (Catalogue x in itemList)
            {
                if (addItem.Contains(x.itemId))
                {
                    //cross check if item exist in cart
                    if (reqCart.Count > 0)
                    {
                        foreach (RequisitionDetail y in reqCart)
                        {
                            if (y.itemId == x.itemId)
                            {
                                tempitem = y;
                                repeatflag = true;
                                break;
                            }
                        }
                        //manipulate ids (for delete purposes)
                        foreach (RequisitionDetail z in reqCart)
                        {
                            if (z.transId > lastID)
                            {
                                lastID = z.transId;
                            }
                        }
                    }
                    // Edit cart item
                    if (repeatflag)
                    {
                        tempitem.requestQty += addQty;
                        tempitem.retrieveQty += addQty;
                    }
                    //Add to cart
                    else
                    {
                        RequisitionDetail reqDetail = new RequisitionDetail();
                        reqDetail.itemId = x.itemId;
                        reqDetail.requestQty = addQty;
                        reqDetail.retrieveQty = addQty;
                        reqDetail.requisitionId = "";
                        reqDetail.transId = lastID + 1;
                        AddCartItem(reqDetail);
                    }
                }
            }
        }

        public void AddCartItem(RequisitionDetail req)
        {
            this.reqCart.Add(req);
        }

        public void RemoveCartItem(RequisitionDetail req)
        {
            this.reqCart.Remove(req);
        }

        public void RemoveAllCartItem()
        {
            this.reqCart.Clear() ;
        }

        public void RemoveCartItem(int reqid)
        {
            RequisitionDetail req = reqCart.Find(x => x.transId == reqid);
            this.reqCart.Remove(req);
        }

        public void EditCartItem(RequisitionDetail req)
        {
            if (reqCart.Contains(req))
            {
                this.reqCart.Remove(req);
                this.reqCart.Add(req);
            }
            else
                this.reqCart.Add(req);
        }

        public RequisitionDetail ItemDetails(int id)
        {
            return this.reqCart[id];
        }

        public List<RequisitionDetail> RequestItemCart()
        {
            return this.reqCart;
        }
    }
}