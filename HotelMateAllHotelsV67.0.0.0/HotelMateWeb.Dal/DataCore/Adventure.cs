//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HotelMateWeb.Dal.DataCore
{
    using System;
    using System.Collections.Generic;
    
    public partial class Adventure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Telephones { get; set; }
        public string PicturePath { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string OpeningClosing { get; set; }
        public string Website { get; set; }
        public bool IsPlaceOfInterest { get; set; }
    }
}
