using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IGuestLedgerService
    {
        IList<GuestLedger> GetAll(int hotelId);
        GuestLedger GetById(int? id);
        GuestLedger Update(GuestLedger guestLedger);
        void Delete(GuestLedger guestLedger);
        GuestLedger Create(GuestLedger guestLedger);
        IQueryable<GuestLedger> GetByQuery(int hotelId);
        GuestLedger GetGuestLedgerByUserNameAndPassword(string domainUsername, string password);
        IList<GuestLedger> GetAllForLogin();
    }
}