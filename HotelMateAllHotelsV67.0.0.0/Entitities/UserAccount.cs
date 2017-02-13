using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class UserAccount : EntityBase
    {
        public virtual User User { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime DatePaid { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string AccountType { get; set; }
    }
}
