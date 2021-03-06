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
    
    public partial class GuestLedger
    {
        public int Id { get; set; }
        public System.DateTime DateCreated { get; set; }
        public int GuestId { get; set; }
        public decimal Amount { get; set; }
        public int Cashier { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int RoomPaymentTypeId { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Guest Guest { get; set; }
        public virtual RoomPaymentType RoomPaymentType { get; set; }
        public virtual Person Person { get; set; }
    }
}
