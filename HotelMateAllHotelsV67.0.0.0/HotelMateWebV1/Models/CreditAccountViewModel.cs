using System.ComponentModel.DataAnnotations;

namespace HotelMateWebV1.Models
{
    public class CreditAccountViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter a Contact Name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Please enter a Telephone Number")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "Please enter a Mobile Number")]
        public string Mobile { get; set; }
        
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Please enter an Address")]
        public string Address { get; set; }
       
        public string CompanyName { get; set; }
      
        public string CompanyAddress { get; set; }
       
        public string CompanyTelephone { get; set; }
        
        public string NatureOfBusiness { get; set; }

        [Required(ErrorMessage = "Please enter an email address")]
        public string Email { get; set; }

        public string Status { get; set; }
    }
}

