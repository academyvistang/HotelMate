using System;
using System.Collections.Generic;
using System.Linq;
using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;

namespace HotelMateWeb.Services.ServiceApi
{
    public class GuestRoomAccountService : IGuestRoomAccountService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IQueryable<GuestRoomAccount> GetAllForGuestByType(int? guestId, int? paymentTypeId)
        {
            return _unitOfWork.GuestRoomAccountRepository.GetByQuery(x => x.GuestRoom.GuestId == guestId.Value && x.PaymentTypeId == paymentTypeId.Value).OrderByDescending(x => x.TransactionDate);
        }

        public IList<GuestRoomAccount> GetAll(int hotelId)
        {
            return _unitOfWork.GuestRoomAccountRepository.Get(x => x.GuestRoom.Guest.HotelId == hotelId).ToList();
        }

        public GuestRoomAccount GetById(int? id)
        {
            return _unitOfWork.GuestRoomAccountRepository.GetByID(id.Value);
        }

        public GuestRoomAccount Update(GuestRoomAccount guestRoomAccount)
        {
            _unitOfWork.GuestRoomAccountRepository.Update(guestRoomAccount);
            _unitOfWork.Save();
            return guestRoomAccount;
        }

        public void Delete(GuestRoomAccount guestRoomAccount)
        {
            _unitOfWork.GuestRoomAccountRepository.Delete(guestRoomAccount);
            _unitOfWork.Save();
        }

        public GuestRoomAccount Create(GuestRoomAccount guestRoomAccount)
        {
            _unitOfWork.GuestRoomAccountRepository.Insert(guestRoomAccount);
            _unitOfWork.Save();
            return guestRoomAccount;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<GuestRoomAccount> GetByQuery(int hotelId)
        {
            return _unitOfWork.GuestRoomAccountRepository.GetByQuery(x => x.GuestRoom.Guest.HotelId == hotelId);
        }
    }
}
