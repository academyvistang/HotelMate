using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class BalanceSheetModel : BaseViewModel
    {
        public DateTime TransactionDate { get; set; }

        public decimal AmountPaidIn { get; set; }

        public decimal AmountPaidOut { get; set; }

        public int? Cashier { get; set; }

        public HotelMateWeb.Dal.DataCore.PaymentMethod PaymentMentMethod { get; set; }

        public HotelMateWeb.Dal.DataCore.RoomPaymentType PaymentType { get; set; }

        public int? PaymentTypeId { get; set; }
    }

    public class ReportViewModel : BaseViewModel
    {
        public IQueryable<HotelMateWeb.Dal.DataCore.Guest> Guests { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Room> Rooms { get; set; }

        public List<HotelMateWeb.Dal.DataCore.Guest> HotelGuests { get; set; }

        public bool RoomHistory { get; set; }

        public List<HotelMateWeb.Dal.DataCore.GuestRoom> RoomHistorys { get; set; }

        public List<IEnumerable<HotelMateWeb.Dal.DataCore.GuestRoom>> RoomOccupancy { get; set; }

        public DateTime? StartDate { get; set; }

        public List<ICollection<HotelMateWeb.Dal.DataCore.GuestRoom>> GuestRooms { get; set; }

        public List<HotelMateWeb.Dal.DataCore.GuestRoomAccount> Accounts { get; set; }

        public List<SoldItemModel> SalesModel { get; set; }

        public List<SoldItemModel> ModelGroupBy { get; set; }

        public List<SoldItemModelAccomodation> ModelGroupByAccomodation { get; set; }

        public List<GuestRoomModel> GroupByList { get; set; }

        public List<EmployeeGroupByModel> EmployeeGroupByList { get; set; }

        public List<BalanceSheetModel> BalanceSheet { get; set; }

        public decimal FullBalance { get; set; }

        public List<HotelMateWeb.Dal.DataCore.PurchaseOrder> InventoryList { get; set; }

        public List<ConciseBalanceSheetModel> ConciseBalanceSheetSheet { get; set; }

        public decimal Tax { get; set; }
        public string ReportName { get; set; }
        public string FileToDownloadPath { get; set; }
        public decimal Discount { get; internal set; }
        public decimal GrandTotal { get; internal set; }
    }
}