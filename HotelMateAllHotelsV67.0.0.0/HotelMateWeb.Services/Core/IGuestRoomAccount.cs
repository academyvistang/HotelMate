using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
 
    public interface IGuestRoomAccountService
    {
        IList<GuestRoomAccount> GetAll(int hotelId);
        IQueryable<GuestRoomAccount> GetByQuery(int hotelId);
        GuestRoomAccount GetById(int? id);
        GuestRoomAccount Update(GuestRoomAccount guestRoomAccount);
        void Delete(GuestRoomAccount guestRoomAccount);
        GuestRoomAccount Create(GuestRoomAccount guestRoomAccount);
        IQueryable<GuestRoomAccount> GetAllForGuestByType(int? guestId, int? paymentTypeId);
    }
}
