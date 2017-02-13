using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IGuestRoomService
    {
        IQueryable<GuestRoom> GetByQuery(int hotelId);
        IList<GuestRoom> GetAll(int hotelId);
        GuestRoom GetById(int? id);
        GuestRoom Update(GuestRoom guestRoom);
        void Delete(GuestRoom guestRoom);
        GuestRoom Create(GuestRoom guestRoom);
    }
}
