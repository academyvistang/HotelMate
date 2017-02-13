using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IAdventureService
    {
        IList<Adventure> GetAll(int hotelId);
        Adventure GetById(int? id);
        Adventure Update(Adventure adventure);
        void Delete(Adventure adventure);
        Adventure Create(Adventure adventure);
        IQueryable<Adventure> GetByQuery(int hotelId);
        Adventure GetAdventureByUserNameAndPassword(string domainUsername, string password);
        IList<Adventure> GetAllForLogin();
    }
}