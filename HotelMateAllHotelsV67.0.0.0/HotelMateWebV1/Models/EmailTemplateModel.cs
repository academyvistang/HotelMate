using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class EmailTemplateModel
    {
        public Guest Guest { get; set; }
        public DateTime ChekinDate { get; set; }
        public DateTime ChekoutDate { get; set; }
        public Decimal InitialDeposit { get; set; }


        public string Vacancy { get; set; }

        public string Occupied { get; set; }

        public string RoomNumber { get; set; }

        public string RoomType { get; set; }

        public string Dirty { get; set; }

        public string PaymentMethod { get; set; }
    }
}