using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class PersonTypeService : IPersonTypeService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();        

        public IList<PersonType> GetAll(int hotelId)
        {
            return _unitOfWork.PersonTypeRepository.Get(x => x.HotelId == hotelId).ToList();
        }

        public PersonType GetById(int? id)
        {
            return _unitOfWork.PersonTypeRepository.GetByID(id.Value);
        }

        public PersonType Update(PersonType personType)
        {
            _unitOfWork.PersonTypeRepository.Update(personType);
            _unitOfWork.Save();
            return personType;
        }

        public void Delete(PersonType personType)
        {
            _unitOfWork.PersonTypeRepository.Delete(personType);
            _unitOfWork.Save();
        }

        public PersonType Create(PersonType personType)
        {
            _unitOfWork.PersonTypeRepository.Insert(personType);
            _unitOfWork.Save();
            return personType;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<PersonType> GetByQuery(int hotelId)
        {
            return _unitOfWork.PersonTypeRepository.GetByQuery(x => x.HotelId == hotelId);
        }
    }
}
