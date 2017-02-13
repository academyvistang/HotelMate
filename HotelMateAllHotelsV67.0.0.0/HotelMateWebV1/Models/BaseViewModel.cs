using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using HotelMateWeb.Services.ServiceApi;
using System.ComponentModel.DataAnnotations;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Helpers;

namespace HotelMateWebV1.Models
{
    public class BaseViewModel
    {
        public BaseViewModel(int hotelId = 1)
        {
            UserName = string.Empty;
            Password = string.Empty;

            var roomService = new RoomService();
            var allRooms = roomService.GetAll(hotelId);

            VacantRooms = allRooms.Count(x => x.RoomStatu.Id == (int)RoomStatusEnum.Vacant);
            OccupiedRooms = allRooms.Count(x => x.RoomStatu.Id == (int)RoomStatusEnum.Occupied);
            ReservedRooms = allRooms.SelectMany(x => x.GuestReservations.Where(y => (y.IsActive && !y.Guest.IsActive && y.StartDate.IsBetween(DateTime.Today.Date,DateTime.Now.AddDays(7))))).Count();

            var bas = allRooms.Select(x => x.RoomType1).Distinct().ToList();
            bas.Insert(0, new HotelMateWeb.Dal.DataCore.RoomType {Id = 0, Name = "-- All --" });
            GlobalRoomTypeList =
                bas.Select(
                    x =>
                    new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString(CultureInfo.InvariantCulture),
                            Selected = x.Id == 0
                        });
        }

        //[Required(ErrorMessage="Please enter a username")]
        public string UserName { get; set; }

        public int VacantRooms { get; set; }

        public int OccupiedRooms { get; set; }

        public int ReservedRooms { get; set; }


        public bool? ItemSaved { get; set; }
        public bool? GuestTransferComplete { get; set; }
        

        //[Required(ErrorMessage = "Please enter a password")]

        public string Password { get; set; }

        public int FutureReservationCount { get; set; }

        public DateTime? CheckinDate { get; set; }
        public DateTime? CheckoutDate { get; set; }
        public IEnumerable<SelectListItem> GlobalRoomTypeList { get; set; }

        public bool? LoginFailed { get; set; }

        public int room_select;

        public DateTime ExpiryDate { get; set; }

        public string HotelName { get; set; }
    }
}