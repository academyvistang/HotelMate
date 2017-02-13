using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IPaymentService
    {
        IList<Payment> GetAllEvery();
        IList<Payment> GetAllIncludeHotel(string include);
        IList<Payment> GetAllHotel();
        IList<Payment> GetAllInclude(string include);
        IList<Payment> GetAll();
        Payment GetById(int? id);
        Payment Update(Payment Payment);
        void Delete(Payment Payment);
        Payment Create(Payment Payment);
        IQueryable<Payment> GetByQuery();
        void Dispose();
    }

}
