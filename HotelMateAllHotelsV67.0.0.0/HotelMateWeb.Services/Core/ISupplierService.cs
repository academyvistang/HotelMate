using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
    public interface ISupplierService
    {
        IList<Supplier> GetAll(int hotelId);
        Supplier GetById(int? id);
        Supplier Update(Supplier supplier);
        void Delete(Supplier supplier);
        Supplier Create(Supplier supplier);
        IQueryable<Supplier> GetByQuery(int hotelId);
    }
}
