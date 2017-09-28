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
    
    public partial class BusinessCorporateAccount
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public Nullable<int> TransactionId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodNote { get; set; }
        public int BusinessAccountId { get; set; }
    
        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual Person Person { get; set; }
        public virtual BusinessAccount BusinessAccount { get; set; }
    }
}
