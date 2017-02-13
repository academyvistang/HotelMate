using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IEscortService
    {
        IList<Escort> GetAll(int hotelId);
        Escort GetById(int? id);
        Escort Update(Escort escort);
        void Delete(Escort escort);
        Escort Create(Escort escort);
        IQueryable<Escort> GetByQuery(int hotelId);
        Escort GetEscortByUserNameAndPassword(string domainUsername, string password);
        IList<Escort> GetAllForLogin();
    }
}