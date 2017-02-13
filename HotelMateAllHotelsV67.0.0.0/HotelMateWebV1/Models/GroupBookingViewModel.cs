using System.Collections.Generic;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWebV1.Models
{
    public class GroupBookingViewModel : BaseViewModel
    {
        public string PageTitle { get; set; }

        public List<Room> GuestRooms { get; set; }

        public string selectedRoomIds { get; set; }

        public RoomBookingViewModel RoomBookingViewModel { get; set; }

        public ICollection<GuestRoom> GuestRoomsCheckedIn { get; set; }

        public int GuestId { get; set; }

        public decimal GuestRefund { get; set; }

        public decimal MaxRefund { get; set; }

        public int GuestRoomId { get; set; }

        public Guest Guest { get; set; }

        public Room Room { get; set; }

        public string[] RoomBookingSelectedValues { get; set; }
        public bool GroupCheckOut { get; internal set; }
    }
}