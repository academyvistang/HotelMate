using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelMateWebV1.Models
{
    public class RoomViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Please enter a room number")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Please enter the number of beds")]
        public int NoOfBeds { get; set; }

        [Required(ErrorMessage = "Please enter the status of the room")]
        public int StatusId { get; set; }

        [Required(ErrorMessage = "Please enter a room price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please enter a room price for business")]
        public decimal BusinessPrice { get; set; }

        //[Required(ErrorMessage = "Please enter a room description")]
        public string Description { get; set; }

        public string Smoking { get; set; }

        [Required(ErrorMessage = "Please enter a room type")]
        [Range(1,double.MaxValue, ErrorMessage="Please select a room type")]
        public int RoomType { get; set; }

        public string ExtNumber { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> RoomTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> RoomStatusList { get; set; }

        public string VideoPath { get; set; }
    }
}