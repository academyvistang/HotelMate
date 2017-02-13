using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IInvoiceStatusTypeService
    {
        IList<InvoiceStatusType> GetAll();
        InvoiceStatusType GetById(int? id);
        InvoiceStatusType Update(InvoiceStatusType InvoiceStatusType);
        void Delete(InvoiceStatusType InvoiceStatusType);
        InvoiceStatusType Create(InvoiceStatusType InvoiceStatusType);
        IQueryable<InvoiceStatusType> GetByQuery();
    }

}
