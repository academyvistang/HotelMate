using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
    public interface IRoomTypeService
    {
        IList<RoomType> GetAll(int hotelId);
        RoomType GetById(int? id);
        RoomType Update(RoomType roomType);
        void Delete(RoomType roomType);
        RoomType Create(RoomType roomType);
        IQueryable<RoomType> GetByQuery(int hotelId);
    }
}
