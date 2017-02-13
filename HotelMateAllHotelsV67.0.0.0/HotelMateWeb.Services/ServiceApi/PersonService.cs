using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class PersonService : IPersonService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Person> GetAllForLogin()
        {
            return _unitOfWork.PersonRepository.Get().ToList();
        }
          
        public IList<Person> GetAll(int hotelId)
        {
            return _unitOfWork.PersonRepository.Get(x => x.HotelId == hotelId).ToList();
        }

        public Person GetPersonByUserNameAndPassword(string domainUsername, string password)
        {
            return _unitOfWork.PersonRepository.Get().FirstOrDefault(x => x.Username.Equals(domainUsername) && x.Password.Equals(password));
        }


        public Person GetById(int? id)
        {
            return _unitOfWork.PersonRepository.GetByID(id.Value);
        }

        public Person Update(Person person)
        {
            _unitOfWork.PersonRepository.Update(person);
            _unitOfWork.Save();
            return person;
        }

        public void Delete(Person person)
        {
            _unitOfWork.PersonRepository.Delete(person);
            _unitOfWork.Save();
        }

        public Person Create(Person person)
        {
            _unitOfWork.PersonRepository.Insert(person);
            _unitOfWork.Save();
            return person;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Person> GetByQuery(int hotelId)
        {
            return _unitOfWork.PersonRepository.GetByQuery(x => x.HotelId == hotelId);
        }
    }
}
