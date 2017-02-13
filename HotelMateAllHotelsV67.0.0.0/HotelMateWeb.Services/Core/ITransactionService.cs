using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface ITransactionService
    {
        IList<Transaction> GetAll(int hotelId);
        Transaction GetById(int? id);
        Transaction Update(Transaction transaction);
        void Delete(Transaction transaction);
        Transaction Create(Transaction transaction);
        IQueryable<Transaction> GetByQuery(int hotelId);
    }

}
