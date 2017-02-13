using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class CheckOutDisplayModel
    {
        public DateTime TransactionDate { get; set; }
        public string DocNumber { get; set; }
        public string Detail { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance { get; set; }

        public int Status { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; }
    }
}