using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class MenuModel
    {
        public int Id { get; set; }

        public List<HotelMateWeb.Dal.DataCore.POSItem> Items { get; set; }

        public object CategoryName { get; set; }
    }

    public class HotelMenuModel
    {
        public int Id { get; set; }

        public List<HotelMateWeb.Dal.DataCore.POSItem> Items { get; set; }

        public List<MenuModel> Menu { get; set; }

        public HotelMateWeb.Dal.DataCore.POSItem MenuItem { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Taxi> Taxis { get; set; }

        public HotelMateWeb.Dal.DataCore.Taxi CarItem { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Adventure> Adventures { get; set; }

        public HotelMateWeb.Dal.DataCore.Adventure Adventure { get; set; }

        public string BookingAgentNumber { get; set; }

        public int RecargeCardPrice { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Escort> Escorts { get; set; }

        public HotelMateWeb.Dal.DataCore.Escort EscortItem { get; set; }
    }
}