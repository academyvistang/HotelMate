using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface ITaxiService
    {
        IList<Taxi> GetAll(int hotelId);
        Taxi GetById(int? id);
        Taxi Update(Taxi taxi);
        void Delete(Taxi taxi);
        Taxi Create(Taxi taxi);
        IQueryable<Taxi> GetByQuery(int hotelId);
        Taxi GetTaxiByUserNameAndPassword(string domainUsername, string password);
        IList<Taxi> GetAllForLogin();
    }
}