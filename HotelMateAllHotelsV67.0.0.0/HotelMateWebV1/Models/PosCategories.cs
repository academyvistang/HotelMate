using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{
    public class PosCategories
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class PosProducts
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public PosCategories PosCategory { get; set; }
    }
}