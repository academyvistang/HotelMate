using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class GuestPlaylistService : IGuestPlaylistService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<GuestPlaylist> GetAll()
        {
            return _unitOfWork.GuestPlaylistRepository.Get().ToList();
        }

        public GuestPlaylist Update(GuestPlaylist existingGpl, List<Movie> selectedVideos)
        {
            var gplForDelete = _unitOfWork.GuestPlaylistRepository.GetByID(existingGpl.Id);

            gplForDelete.Movies.Clear();

            _unitOfWork.GuestPlaylistRepository.Delete(gplForDelete);

            _unitOfWork.Save();

            var newGPL = new GuestPlaylist { Description = existingGpl.Description, GuestId = existingGpl.GuestId, Name = existingGpl.Name };

            return Create(newGPL, selectedVideos);
        }


        public GuestPlaylist Create(GuestPlaylist gpl, List<Movie> selectedVideos)
        {
            var ids = selectedVideos.Select(x => x.Id).ToList();
            var m = _unitOfWork.MovieRepository.Get().Where(x => ids.Contains(x.Id)).ToList();
            gpl.Movies = m;

            _unitOfWork.GuestPlaylistRepository.Insert(gpl);
            _unitOfWork.Save();
            return gpl;
        }


        public GuestPlaylist GetById(int? id)
        {
            return _unitOfWork.GuestPlaylistRepository.GetByID(id.Value);
        }

        public GuestPlaylist Update(GuestPlaylist guestPlaylist)
        {
            _unitOfWork.GuestPlaylistRepository.Update(guestPlaylist);
            _unitOfWork.Save();
            return guestPlaylist;
        }

        public void Delete(GuestPlaylist guestPlaylist)
        {
            _unitOfWork.GuestPlaylistRepository.Delete(guestPlaylist);
            _unitOfWork.Save();
        }

        public GuestPlaylist Create(GuestPlaylist guestPlaylist)
        {
            _unitOfWork.GuestPlaylistRepository.Insert(guestPlaylist);
            _unitOfWork.Save();
            return guestPlaylist;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<GuestPlaylist> GetByQuery()
        {
            return _unitOfWork.GuestPlaylistRepository.GetByQuery();
        }
    }
}
