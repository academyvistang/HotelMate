using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class RoomTypeViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

    }
}