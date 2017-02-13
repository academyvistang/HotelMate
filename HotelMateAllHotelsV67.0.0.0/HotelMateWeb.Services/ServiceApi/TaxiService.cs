using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class TaxiService : ITaxiService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Taxi> GetAllForLogin()
        {
            return _unitOfWork.TaxiRepository.Get().ToList();
        }

        public IList<Taxi> GetAll(int hotelId)
        {
            return _unitOfWork.TaxiRepository.Get().ToList();
        }

        public Taxi GetTaxiByUserNameAndPassword(string domainUsername, string password)
        {
            return null;
        }


        public Taxi GetById(int? id)
        {
            return _unitOfWork.TaxiRepository.GetByID(id.Value);
        }

        public Taxi Update(Taxi taxi)
        {
            _unitOfWork.TaxiRepository.Update(taxi);
            _unitOfWork.Save();
            return taxi;
        }

        public void Delete(Taxi taxi)
        {
            _unitOfWork.TaxiRepository.Delete(taxi);
            _unitOfWork.Save();
        }

        public Taxi Create(Taxi taxi)
        {
            _unitOfWork.TaxiRepository.Insert(taxi);
            _unitOfWork.Save();
            return taxi;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Taxi> GetByQuery(int hotelId)
        {
            return _unitOfWork.TaxiRepository.GetByQuery();
        }
    }
}
