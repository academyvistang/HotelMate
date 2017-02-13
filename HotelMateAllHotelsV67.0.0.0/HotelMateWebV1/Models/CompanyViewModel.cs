using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class CompanyViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please Enter A Company Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter Contact Name")]
        public string ContactName { get; set; }

        public string Telephone { get; set; }        
        public string Mobile { get; set; }        
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTelephone { get; set; }
        public string CompanyAddress { get; set; }
        public string NatureOfBusiness { get; set; }
        public string Email { get; set; }

    }
}