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
    
    public partial class RoomPaymentType
    {
        public RoomPaymentType()
        {
            this.GuestLedgers = new HashSet<GuestLedger>();
            this.GuestRoomAccounts = new HashSet<GuestRoomAccount>();
            this.Payments = new HashSet<Payment>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int PaymentStatusId { get; set; }
    
        public virtual ICollection<GuestLedger> GuestLedgers { get; set; }
        public virtual ICollection<GuestRoomAccount> GuestRoomAccounts { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual RoomPaymentTypeStatu RoomPaymentTypeStatu { get; set; }
    }
}
