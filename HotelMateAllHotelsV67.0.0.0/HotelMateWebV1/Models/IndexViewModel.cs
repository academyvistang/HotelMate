using POSService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class IndexViewModel
    {
        public IEnumerable<StockItem> productsList { get; set; }
        public IEnumerable<Category> categoriesList { get; set; }

        public IEnumerable<Guest> CurrentGuests { get; set; }

        public IEnumerable<HotelMateWeb.Dal.DataCore.Person> CurrentCashiers { get; set; }


        public int GuestId { get; set; }

        public int PersonId { get; set; }

        public string Terminal { get; set; }

        public string Table { get; set; } 

        public int ProductsAlerts { get; set; }

        public int TableId { get; set; }

        public List<HotelMateWeb.Dal.DataCore.TableItem> ExistingList { get; set; }

        public bool Retrieve { get; set; }
    }
}