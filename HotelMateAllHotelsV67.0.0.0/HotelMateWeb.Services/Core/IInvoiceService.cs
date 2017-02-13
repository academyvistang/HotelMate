using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IInvoiceService
    {
        IList<Invoice> GetAll();
        Invoice GetById(int? id);
        Invoice Update(Invoice Invoice);
        void Delete(Invoice Invoice);
        Invoice Create(Invoice Invoice);
        IQueryable<Invoice> GetByQuery();
    }

}
