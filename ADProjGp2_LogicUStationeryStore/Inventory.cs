//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ADProjGp2_LogicUStationeryStore
{
    using System;
    using System.Collections.Generic;
    
    public partial class Inventory
    {
        public string itemId { get; set; }
        public int disburseQuantity { get; set; }
        public int storeQuantity { get; set; }
        public int adjQuantity { get; set; }
    
        public virtual Catalogue Catalogue { get; set; }
    }
}
