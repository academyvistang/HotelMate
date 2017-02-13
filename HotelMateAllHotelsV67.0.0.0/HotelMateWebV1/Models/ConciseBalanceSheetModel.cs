using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class ConciseBalanceSheetModel
    {
        public DateTime ActualDate { get; set; }

        public decimal TotalRecieveable { get; set; }

        public decimal TotalPayaeble { get; set; }
    }
}