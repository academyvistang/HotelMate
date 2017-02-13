using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelMateWebV1.Models
{
    public class Expenses 
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ExpensesViewModel : BaseViewModel
    {
        public int Id { get; set; }
       
        [Required(ErrorMessage = "Please select staff")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter an expense amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please enter an transaction date")]
        public DateTime TransactionDate { get; set; }
       
        public bool IsActive { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StaffList { get; set; }
    }
}