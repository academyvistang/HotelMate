using System.ComponentModel.DataAnnotations;

namespace HotelMateWebV1.Models
{
    public class GuestViewModel : BaseViewModel
    {
        public int CompanyId { get; set; }
        public int Id { get; set; }
        [Required(ErrorMessage = "Please Enter Guest Fullname")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please Enter Guest Telephone")]
        public string Telephone { get; set; }

        public string Mobile { get; set; }

        public int CountryId { get; set; }

        [Required(ErrorMessage = "Please Enter Guest Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please Enter Guest Email")]
        public string Email { get; set; }
        
        public string CarDetails { get; set; }

        public string Status { get; set; }

        public string PassportNumber { get; set; }

        public string IsActive { get; set; }

        public string Notes { get; set; }

        public bool IsChild { get; set; }

    }
}