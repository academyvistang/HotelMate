using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;


namespace HotelMateWeb.Services.ServiceApi
{

    public class GuestReservationService : IGuestReservationService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<GuestReservation> GetAll(int hotelId)
        {
            return _unitOfWork.GuestReservationRepository.Get(x => x.Guest.HotelId == hotelId).ToList();
        }

        public GuestReservation GetById(int? id)
        {
            return _unitOfWork.GuestReservationRepository.GetByID(id.Value);
        }

        public GuestReservation Update(GuestReservation guestReservation)
        {
            _unitOfWork.GuestReservationRepository.Update(guestReservation);
            _unitOfWork.Save();
            return guestReservation;
        }

        public void Delete(GuestReservation guestReservation)
        {
            _unitOfWork.GuestReservationRepository.Delete(guestReservation);
            _unitOfWork.Save();
        }

        public GuestReservation Create(GuestReservation guestReservation)
        {
            _unitOfWork.GuestReservationRepository.Insert(guestReservation);
            _unitOfWork.Save();
            return guestReservation;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<GuestReservation> GetByQuery(int hotelId)
        {
            return _unitOfWork.GuestReservationRepository.GetByQuery(x => x.Guest.HotelId == hotelId);
        }
    }
}
