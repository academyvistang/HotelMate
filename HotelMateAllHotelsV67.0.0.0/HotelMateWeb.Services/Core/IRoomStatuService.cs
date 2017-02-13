using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
    public interface IRoomStatuService
    {
        IList<RoomStatu> GetAll(int hotelId);
        RoomStatu GetById(int? id);
        RoomStatu Update(RoomStatu roomStatu);
        void Delete(RoomStatu roomStatu);
        RoomStatu Create(RoomStatu roomStatu);
        IQueryable<RoomStatu> GetByQuery(int hotelId);
    }
}
