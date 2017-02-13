//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HotelMateWeb.Dal.DataCore
{
    using System;
    using System.Collections.Generic;
    
    public partial class Batch
    {
        public Batch()
        {
            this.DamagedBatchItems = new HashSet<DamagedBatchItem>();
            this.DistributionPointItems = new HashSet<DistributionPointItem>();
        }
    
        public int Id { get; set; }
        public System.DateTime BatchDate { get; set; }
        public int DistributionPointId { get; set; }
        public int PurchaseOrderItemId { get; set; }
        public int QuantityTransferred { get; set; }
        public int POSItemId { get; set; }
    
        public virtual DistributionPoint DistributionPoint { get; set; }
        public virtual PurchaseOrderItem PurchaseOrderItem { get; set; }
        public virtual ICollection<DamagedBatchItem> DamagedBatchItems { get; set; }
        public virtual ICollection<DistributionPointItem> DistributionPointItems { get; set; }
    }
}
