using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HotelMateWeb.Dal.DataCore;
using System.Web.WebPages.Html;
using System.Linq;

namespace HotelMateWebV1.Models
{
    public class GuestRoomAccountViewModel : BaseViewModel
    {
        public GuestRoomAccount GuestRoomAccount { get; set; }

        public DateTime PreviousCheckOutDate { get; set; }

        [Required(ErrorMessage = "Please enter a new checkout date")]
        public DateTime NewCheckOutDate { get; set; }

        public string Notes { get; set; }

        public Guest Guest { get; set; }

        public int RoomId { get; set; }

        public Room Room { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> RoomAccounts;

        public List<Room> Rooms { get; set; }

        public int PaymentTypeId { get; set; }

        public List<RoomPaymentType> PaymentTypeList { get; set; }

        public string ImageUrl { get; set; }

        public GuestRoom GuestRoom { get; set; }

        [DisplayName("Room Number")]
        public string GuestRoomNumber { get; set; }

        [DisplayName("Full Name")]
        public string GuestName { get; set; }

        [DisplayName("Arrival Date")]
        public string ArrivalDate { get; set; }

        [DisplayName("Departure Date")]
        public string DepartureDate { get; set; }

        [DisplayName("No. Of Nights")]
        public string NoOfNight { get; set; }

        [DisplayName("Guests")]
        public string NoOfPersons { get; set; }

        [DisplayName("Currency")]
        public string Currency { get; set; }

        [DisplayName("Discounts")]
        public string Discounts { get; set; }

        [DisplayName("Room Rate")]
        public string RoomRate { get; set; }

        [DisplayName("Bill No.")]
        public string BillNo { get; set; }

        [DisplayName("Guest Sign        ________________________________________")]
        public string GuestSign { get; set; }

        [DisplayName("Reception Sign________________________________________")]
        public string ReceptionSign { get; set; }

        public string PaymentType { get; set; }

        public int CurrentPageIndex { get; set; }

        public string NextPageUrl { get; set; }

        public string PreviousPageUrl { get; set; }

        public Helpers.PaginatedList<HotelMateWeb.Dal.DataCore.GuestRoomAccount> PaginatedList { get; set; }

        public List<HotelMateWeb.Dal.DataCore.GuestRoom> GuestRoomsForCheckout { get; set; }

        public int GuestRoomId { get; set; }
        
        public decimal AdjustedAmount { get; set; }

        public string PaymentTypeString { get; set; }

        public int PaymentMethodId { get; set; }

        public string PaymentMethodNote { get; set; }
        public int NewGuestRoomId { get; set; }

        private List<PaymentMethod> PaymentMethodList = new List<PaymentMethod>{new PaymentMethod{ Id = 1, Description = "CASH" },new PaymentMethod{ Id = 2, Description = "CHEQUE"},
            new PaymentMethod{ Id = 3, Description = "POS"}};
     

        public int gId { get; set; }

        public IEnumerable<SelectListItem> PaymentMethods
        {
            get
            {
                var numbers = (from p in PaymentMethodList.ToList()
                               select new SelectListItem
                               {
                                   Text = p.Description.ToString(),
                                   Value = p.Id.ToString()
                               });
                return numbers.ToList();
            }
        }

        public List<SoldItemsAll> ItemmisedItems { get; set; }

        public string Feedback { get; set; }

        public List<CinemaModel> CinemaList { get; set; }

        public List<string> Categories { get; set; }

        public string AppDataPath { get; set; }

        public string SearchName { get; set; }

        public bool DoRefund { get; set; }

        public int GuestAccountId { get; set; }

        public List<CheckOutDisplayModel> DisplayList { get; set; }

        public List<string> TermsAndConditions { get; set; }

        public List<string> Acknowledge { get; set; }

        public List<HotelMateWeb.Dal.DataCore.GuestRoom> GuestRooms { get; set; }
        public bool GroupCheckout { get; internal set; }
        public int? CompanyGuest { get; internal set; }
        public int RememberToChangeMainRoom { get; set; }
    }
}

