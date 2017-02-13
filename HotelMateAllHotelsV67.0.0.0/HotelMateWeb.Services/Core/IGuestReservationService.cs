using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IGuestReservationService
    {
        IList<GuestReservation> GetAll(int hotelId);
        IQueryable<GuestReservation> GetByQuery(int hotelId);
        GuestReservation GetById(int? id);
        GuestReservation Update(GuestReservation guestReservation);
        void Delete(GuestReservation guestReservation);
        GuestReservation Create(GuestReservation guestReservation);
    }
    
}
