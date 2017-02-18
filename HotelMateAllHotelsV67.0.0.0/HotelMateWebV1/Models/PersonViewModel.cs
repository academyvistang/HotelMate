using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class PersonViewModel : BaseViewModel
    {
        public int PersonID { get; set; }

        public int GuestId { get; set; }


        [Required(ErrorMessage = "Please Enter An Email")]
        //[EmailAddress(ErrorMessage = "Please Enter A Valid Email")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter A Password")]
        public string Password { get; set; }

        public string GuestName { get; set; }

        [Required(ErrorMessage = "Please Enter A Firstname")]        
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter A Lastname")]
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public int PersonTypeId { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PersonTypes { get; set; }

        public HotelMateWeb.Dal.DataCore.Person Person { get; set; }

        public decimal TotalHotelReceivable { get; set; }

        public decimal TotalBarReceivable { get; set; }

        public decimal TotalSales { get; set; }

        public string Title { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }       
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Guardian { get; set; }
        public string GuardianAddress { get; set; }
        public string PreviousEmployer { get; set; }
        public DateTime PreviousEmployerStartDate { get; set; }
        public DateTime PreviousEmployerEndDate { get; set; }
        public string ReasonForLeaving { get; set; }
        public string Notes { get; set; }
        public int SupplierId { get; set; }
        public bool CanCloseTill { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string JobTitle { get; set; }
        public string CityState { get; set; }
        public string WorkAddress { get; set; }
        
        public string Department { get; set; }
        public string IdNumber { get; set; }
        public string BankDetails { get; set; }
        public string AccountNumber { get; set; }
        public Decimal Salary { get; set; }
        public Byte[] EmployeePicture { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string PlaceOfBirth { get; set; }
        public string NoOfChildren { get; set; }
        public string Qualification { get; set; }
        public string PicturePath { get; set; }

    }
}