using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSService.Entities
{
    public class StockItem
    {
        public bool CookedFood { get; set; }

        public bool KitchenOnly { get; set; }

        // Properties
        public string Barcode { get; set; }

        public int CategoryId { get; set; }

        public string Description { get; set; }

        public int HotelId { get; set; }

       // [Browsable(false), Column("Id"), Key]
        public long Id { get; set; }

        public bool IsActive { get; set; }

        public int NotNumber { get; set; }

        public decimal OrigPrice { get; set; }

        public string PicturePath { get; set; }

        public int Quantity { get; set; }

        public string Status { get; set; }

        public string StockItemName { get; set; }

        public decimal UnitPrice { get; set; }

        public int TotalQuantity { get; set; }

    }

 

}
