using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IGuestService 
    {
        IList<Guest> GetAll(int hotelId);
        Guest GetById(int? id);
        Guest Update(Guest guest);
        void Delete(Guest guest);
        Guest Create(Guest guest);
        IQueryable<Guest> GetByQuery(int hotelId);
        IQueryable<Guest> GetByQueryAll(int hotelId); 
 

    }
}
