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
    
    public partial class FoodMatrix
    {
        public int Id { get; set; }
        public int RawItemId { get; set; }
        public int FoodItemId { get; set; }
        public int Qty { get; set; }
    
        public virtual StockItem StockItem { get; set; }
        public virtual StockItem StockItem1 { get; set; }
    }
}
