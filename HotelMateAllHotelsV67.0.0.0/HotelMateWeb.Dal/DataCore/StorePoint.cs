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
    
    public partial class StorePoint
    {
        public StorePoint()
        {
            this.Batches = new HashSet<Batch>();
            this.StorePointItems = new HashSet<StorePointItem>();
            this.DamagedBatchItems = new HashSet<DamagedBatchItem>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StorePointManager { get; set; }
        public bool IsSales { get; set; }
    
        public virtual ICollection<Batch> Batches { get; set; }
        public virtual ICollection<StorePointItem> StorePointItems { get; set; }
        public virtual ICollection<DamagedBatchItem> DamagedBatchItems { get; set; }
        public virtual Person Person { get; set; }
    }
}