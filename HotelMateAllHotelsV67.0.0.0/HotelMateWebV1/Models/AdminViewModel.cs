using HotelMateWeb.Dal.DataCore;
using System.Collections.Generic;
namespace HotelMateWebV1.Models
{
    public class AdminViewModel
    {
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.Guest> PaginatedGuestList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.GuestReservation> PaginatedGuestReservationList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.GuestRoomAccount> PaginatedGuestRoomAccountList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.BusinessAccount> PaginatedBusinessAccountList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.Transaction> PaginatedTransactionList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.GuestRoom> PaginatedGuestRoomList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.Room> PaginatedRoomList { get; set; }
        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.RoomTransferHistory> PaginatedRoomTranferList { get; set; }

        public decimal MonthlyCreditSales { get; set; }
        public decimal MonthlyDebitSales { get; set; }


        public decimal ProfitSales { get; set; }

        public int NumberOfGuests { get; set; }

        public int ShortTermStay { get; set; }

        public int LongTermStay { get; set; }

        public int ReservationCount { get; set; }

        public System.Collections.Generic.IEnumerable<HotelMateWeb.Dal.DataCore.GuestRoom> GuestSales { get; set; }

        public decimal RoomOnlySales { get; set; }

        public IEnumerable<GuestRoom> GuestRooms { get; set; }
    }
}