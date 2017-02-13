using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Models
{
    public class PurchaseOrderModel
    {
        [Display(Name="Description")]
        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }

        [Display(Name = "Supplier Reference")]
        public string SupplierReference { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        public IEnumerable<SelectListItem> selectList { get; set; }
        public int Id { get; set; }

        public bool? Saved { get; set; }

        public IEnumerable<SelectListItem> selectListStore { get; set; }

        public List<HotelMateWeb.Dal.DataCore.PurchaseOrderItem> Items { get; set; }

        public IList<HotelMateWeb.Dal.DataCore.StockItem> StockItems { get; set; }

        [Display(Name = "Net Value")]
        public decimal Value { get; set; }

        public List<HotelMateWeb.Dal.DataCore.StockItem> DamagedGoods { get; set; }

        public bool Recieved { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }       

        //[Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Please enter a selling price")]
        [Range(1, 99999999.99, ErrorMessage = "Value must be between 0 - 9,9999999.99")]
        public decimal UnitPrice { get; set; }
        
        public string PicturePath { get; set; }

        public string Status { get; set; }

       // [Required(ErrorMessage = "Please enter a Quantity")]
       // [Range(1, 999999.99, ErrorMessage = "Value must be between 0 - 9,9999999.99")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Please enter a Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter a buying price")]
        [Range(0, 99999999.99, ErrorMessage = "Value must be between 0 - 9,9999999.99")]
        public decimal OrigPrice { get; set; }

        [Required(ErrorMessage = "Please enter a Notification Number")]
        [Range(0, 999999.99, ErrorMessage = "Value must be between 0 - 9,9999999.99")]
        public int NotNumber { get; set; }

        public string NotStatus { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        public string StockItemName { get; set; }
       
        public int TotalQuantity { get; set; }

        public string Barcode { get; set; }

        public string Picture { get; set; }

        public int HotelId { get; set; }

        public IEnumerable<SelectListItem> selectList { get; set; }

        public bool? Saved { get; set; }


        [Display(Name="Cooked Food")]
        public bool CookedFood { get; set; }

        public decimal? Price { get; set; }

        public bool KitchenOnly { get; set; }
    }

    public class ItemIndexModel
    {
        public IEnumerable<ItemModel> ItemList { get; set; }


        public HotelMateWeb.Dal.DataCore.PurchaseOrder POItem { get; set; }

        public List<HotelMateWeb.Dal.DataCore.PurchaseOrder> POItemList { get; set; }

        public bool CanCreatePO { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Store> StoreList { get; set; }

        public HotelMateWeb.Dal.DataCore.Person ThisUser { get; set; }

        public int ThisUserId { get; set; }

        public int StoreId { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Store> allStores { get; set; }

        public List<HotelMateWeb.Dal.DataCore.DistributionPoint> DistributionPoints { get; set; }

        public HotelMateWeb.Dal.DataCore.DistributionPoint DistributionPoint { get; set; }

        public HotelMateWeb.Dal.DataCore.Store Store { get; set; }

        public bool CanRecordDamages { get; set; }

        public int PoId { get; set; }
    }
}