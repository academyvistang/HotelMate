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
    
    public partial class StockItemHotel
    {
        public StockItemHotel()
        {
            this.StoreItems = new HashSet<StoreItem>();
        }
    
        public int Id { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public string Description { get; set; }
        public string PicturePath { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public int Quantity { get; set; }
        public int NotNumber { get; set; }
        public string Name { get; set; }
        public int TotalQuantity { get; set; }
        public string Barcode { get; set; }
    
        public virtual ICollection<StoreItem> StoreItems { get; set; }
    }
}
