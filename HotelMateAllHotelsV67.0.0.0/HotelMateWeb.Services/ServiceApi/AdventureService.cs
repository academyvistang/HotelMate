using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class AdventureService : IAdventureService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Adventure> GetAllForLogin()
        {
            return _unitOfWork.AdventureRepository.Get().ToList();
        }

        public IList<Adventure> GetAll(int hotelId)
        {
            return _unitOfWork.AdventureRepository.Get().ToList();
        }

        public Adventure GetAdventureByUserNameAndPassword(string domainUsername, string password)
        {
            return null;
        }


        public Adventure GetById(int? id)
        {
            return _unitOfWork.AdventureRepository.GetByID(id.Value);
        }

        public Adventure Update(Adventure adventure)
        {
            _unitOfWork.AdventureRepository.Update(adventure);
            _unitOfWork.Save();
            return adventure;
        }

        public void Delete(Adventure adventure)
        {
            _unitOfWork.AdventureRepository.Delete(adventure);
            _unitOfWork.Save();
        }

        public Adventure Create(Adventure adventure)
        {
            _unitOfWork.AdventureRepository.Insert(adventure);
            _unitOfWork.Save();
            return adventure;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Adventure> GetByQuery(int hotelId)
        {
            return _unitOfWork.AdventureRepository.GetByQuery();
        }
    }
}
