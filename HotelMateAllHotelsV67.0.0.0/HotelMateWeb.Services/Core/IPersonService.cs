using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IPersonService
    {
        IList<Person> GetAll(int hotelId);
        Person GetById(int? id);
        Person Update(Person person);
        void Delete(Person person);
        Person Create(Person person);
        IQueryable<Person> GetByQuery(int hotelId);
        Person GetPersonByUserNameAndPassword(string domainUsername, string password);
        IList<Person> GetAllForLogin();
    }
}
