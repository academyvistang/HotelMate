using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }

        [Required( ErrorMessage="Please enter a category name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public bool? Saved { get; set; }

    }

    public class CategoryIndexModel
    {
        public IEnumerable<CategoryModel> CategoryList { get; set; }
    }
}