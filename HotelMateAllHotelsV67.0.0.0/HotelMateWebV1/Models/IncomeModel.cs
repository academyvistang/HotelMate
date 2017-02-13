using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Models
{
    public class IncomeModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter a date")]
        public DateTime IncomeDate { get; set; }

        public bool IsActive { get; set; }

        [DisplayName("Income Type")]
        [Required(ErrorMessage = "Please enter an Income Type")]
        [Range(1, 99999999.99, ErrorMessage = "Value must be between 0 - 9")]
        public int IncomeTypeId { get; set; }

        //public IEnumerable<SelectListIncome> selectList { get; set; }

        public bool? Saved { get; set; }

        public decimal Amount { get; set; }

        public IEnumerable<SelectListItem> selectList { get; set; }

        public int StaffId { get; set; }
    }

    public class IncomeIndexModel
    {
        public IEnumerable<IncomeModel> IncomeList { get; set; }

    }

    public class EmployeeGroupByModel
    {

        public HotelMateWeb.Dal.DataCore.Person Person { get; set; }

        public List<HotelMateWeb.Dal.DataCore.EmployeeShift> ItemList { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class AccomodationModel
    {

        public DateTime DateSold { get; set; }

        public HotelMateWeb.Dal.DataCore.Person Person { get; set; }

        public List<HotelMateWeb.Dal.DataCore.GuestRoomAccount> ItemList { get; set; }



        public HotelMateWeb.Dal.DataCore.GuestRoom GuestRoom { get; set; }
    }

    public class PersonAccomodationModel
    {
        public string DateSold { get; set; }

        public HotelMateWeb.Dal.DataCore.GuestRoom GuestRoom { get; set; }

        public decimal TotalPaidByGuest { get; set; }

        public decimal TotalPaidToGuest { get; set; }

        public decimal GuestTotal { get; set; }

        public decimal Cash { get; set; }

        public decimal Cheque { get; set; }

        public decimal CreditCard { get; set; }

        public HotelMateWeb.Dal.DataCore.Person Person { get; set; }

        public List<TerminalModel> Terminal { get; set; }
    }

    public class PersonDateModel
    {
        public List<HotelMateWeb.Dal.DataCore.GuestRoomAccount> ItemLst { get; set; }

        public string DateSoldString { get; set; }
    }

    public class SalesPersonModel
    {

        public List<HotelMateWeb.Dal.DataCore.GuestRoomAccount> ItemLst { get; set; }

        public HotelMateWeb.Dal.DataCore.Person Person { get; set; }
    }
}