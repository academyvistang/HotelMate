using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IGuestPlaylistService
    {
        IList<GuestPlaylist> GetAll();
        GuestPlaylist GetById(int? id);
        GuestPlaylist Update(GuestPlaylist GuestPlaylist);
        void Delete(GuestPlaylist GuestPlaylist);
        GuestPlaylist Create(GuestPlaylist GuestPlaylist);
        IQueryable<GuestPlaylist> GetByQuery();

        GuestPlaylist Create(GuestPlaylist gpl, List<Movie> selectedVideos);

        GuestPlaylist Update(GuestPlaylist existingGpl, List<Movie> selectedVideos);
    }

}
