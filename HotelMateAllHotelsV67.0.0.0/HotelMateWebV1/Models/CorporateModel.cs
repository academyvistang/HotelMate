using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWebV1.Models
{
    public class CorporateModel : BaseViewModel
    {
        public string Fullname { get; set; }

        public List<Room> RoomsList { get; set; }
        public List<GuestRoom> GuestRoomsList { get; set; }

        public List<Guest> GuestsList { get; set; }

        public List<Person> PersonsList { get; set; }

        public List<BusinessAccount> CompanyList { get; set; }

        public List<Supplier> SuppliersList { get; set; }

        public BusinessAccount Company { get; set; }

        public List<GuestRoomAccount> RoomAccounts { get; set; }

        public List<SoldItemsAll> ItemmisedItems { get; set; }

        public int PaymentTypeId { get; set; }

        public BusinessCorporateAccount BusinessCorporateAccount { get; set; }



        public Helpers.Enums.PaymentMethodEnum PaymentMethodId { get; set; }

        public string PaymentMethodNote { get; set; }
    }
}