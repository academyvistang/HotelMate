using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IRoomService
    {
        IList<Room> GetAll(int hotelId);
        Room GetById(int? id);
        Room Update(Room room);
        void Delete(Room room);
        Room Create(Room room);
        IQueryable<Room> GetByQuery(int hotelId);

    }
}
