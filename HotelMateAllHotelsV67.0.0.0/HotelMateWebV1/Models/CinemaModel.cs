using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Models
{
    public class MusicVideoModel
    {

        public string FullPath { get; set; }

        public string FullPathImage { get; set; }

        public string FullPathWebM { get; set; }

        public string MovieName { get; set; }
    }

    public class CinemaModel
    {
        public IEnumerable<SelectListItem> AllBuildings { get; set; }
        public IEnumerable<SelectListItem> CurrentBuildings { get; set; }

        public int[] AllBuildingIds { get; set; }
        public int[] CurrentBuildingIds { get; set; }

        public string FilmName { get; set; }

        [Required(ErrorMessage="Please enter a name")]
        public string PlaylistName { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        public string VideoName { get; set; }

        public bool AdultOnly { get; set; }

        public string PlaylistDescription { get; set; } 





        public string Year { get; set; }

        public string Starring { get; set; }

        public string Genre { get; set; }

        public int Id { get; set; }

        public string VideoPath { get; set; }

        public int FilmId { get; set; }

        public string MovieName { get; set; }

        public string PosterPath { get; set; }

        public string MovieType { get; set; }



        public string FileName { get; set; }

        public string FullPath { get; set; }

        public string FullPathWebM { get; set; }

        public int CategoryId { get; set; }



        public string FullPathImage { get; set; }

        public List<MusicVideoModel> MusicVideos { get; set; }

        public int GuestId { get; set; }

        public int PlayListId { get; set; }

        



        public string GuestName { get; set; }

        public List<HotelMateWeb.Dal.DataCore.GuestPlaylist> PlaylistList { get; set; }

        public HotelMateWeb.Dal.DataCore.Movie Movie { get; set; }



        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}