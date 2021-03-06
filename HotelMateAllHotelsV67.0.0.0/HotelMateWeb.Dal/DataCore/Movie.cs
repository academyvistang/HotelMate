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
    
    public partial class Movie
    {
        public Movie()
        {
            this.GuestPlaylists = new HashSet<GuestPlaylist>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string Starring { get; set; }
        public string Year { get; set; }
        public string Filename { get; set; }
        public bool AdultOnly { get; set; }
    
        public virtual MovieCategory MovieCategory { get; set; }
        public virtual ICollection<GuestPlaylist> GuestPlaylists { get; set; }
    }
}
