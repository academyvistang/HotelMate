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
    
    public partial class PurchaseOrder
    {
        public PurchaseOrder()
        {
            this.Invoices = new HashSet<Invoice>();
            this.PurchaseOrderItems = new HashSet<PurchaseOrderItem>();
        }
    
        public int Id { get; set; }
        public string Description { get; set; }
        public System.DateTime OrderDate { get; set; }
        public int RaisedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public decimal NetValue { get; set; }
        public decimal BaseNetValue { get; set; }
        public bool IsActive { get; set; }
        public bool InvoiceRaised { get; set; }
        public bool GoodsRecieved { get; set; }
        public bool GoodsBought { get; set; }
        public bool Completed { get; set; }
        public string Notes { get; set; }
        public string SupplierInvoice { get; set; }
        public string InvoicePath { get; set; }
        public bool IsRawItem { get; set; }
        public bool TransferDone { get; set; }
        public Nullable<int> TotalNumberOfItems { get; set; }
    
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}
