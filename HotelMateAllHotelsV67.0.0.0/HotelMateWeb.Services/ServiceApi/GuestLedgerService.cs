using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class GuestLedgerService : IGuestLedgerService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<GuestLedger> GetAllForLogin()
        {
            return _unitOfWork.GuestLedgerRepository.Get().ToList();
        }

        public IList<GuestLedger> GetAll(int hotelId)
        {
            return _unitOfWork.GuestLedgerRepository.Get().ToList();
        }

        public GuestLedger GetGuestLedgerByUserNameAndPassword(string domainUsername, string password)
        {
            return null;
        }


        public GuestLedger GetById(int? id)
        {
            return _unitOfWork.GuestLedgerRepository.GetByID(id.Value);
        }

        public GuestLedger Update(GuestLedger guestLedger)
        {
            _unitOfWork.GuestLedgerRepository.Update(guestLedger);
            _unitOfWork.Save();
            return guestLedger;
        }

        public void Delete(GuestLedger guestLedger)
        {
            _unitOfWork.GuestLedgerRepository.Delete(guestLedger);
            _unitOfWork.Save();
        }

        public GuestLedger Create(GuestLedger guestLedger)
        {
            _unitOfWork.GuestLedgerRepository.Insert(guestLedger);
            _unitOfWork.Save();
            return guestLedger;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<GuestLedger> GetByQuery(int hotelId)
        {
            return _unitOfWork.GuestLedgerRepository.GetByQuery();
        }
    }
}
