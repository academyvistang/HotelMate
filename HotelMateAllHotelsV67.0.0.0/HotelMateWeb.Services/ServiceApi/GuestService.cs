using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class GuestService : IGuestService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Guest> GetAll(int hotelId)
        {
            return _unitOfWork.GuestRepository.Get(x => x.HotelId == hotelId).ToList();
        }

        public Guest GetById(int? id)
        {
            return _unitOfWork.GuestRepository.GetByID(id.Value);
        }

        public Guest Update(Guest guest)
        {
            _unitOfWork.GuestRepository.Update(guest);
            _unitOfWork.Save();
            return guest;
        }

        public void Delete(Guest guest)
        {
            _unitOfWork.GuestRepository.Delete(guest);
            _unitOfWork.Save();
        }

        public Guest Create(Guest guest)
        {
            _unitOfWork.GuestRepository.Insert(guest);
            _unitOfWork.Save();
            return guest;
        }       

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Guest> GetByQuery(int hotelId)
        {
            return _unitOfWork.GuestRepository.GetByQuery(x => x.HotelId == hotelId);
        }

        public IQueryable<Guest> GetByQueryAll(int hotelId)
        {
            return _unitOfWork.GuestRepository.GetByQuery(x => x.HotelId == hotelId, null, "GuestRooms,GuestRooms.GuestRoomAccounts");
        }

    }
}
