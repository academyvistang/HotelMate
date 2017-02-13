using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class InvoiceService : IInvoiceService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Invoice> GetAll()
        {
            return _unitOfWork.InvoiceRepository.Get().ToList();
        }

        public Invoice GetById(int? id)
        {
            return _unitOfWork.InvoiceRepository.GetByID(id.Value);
        }

        public Invoice Update(Invoice invoice)
        {
            _unitOfWork.InvoiceRepository.Update(invoice);
            _unitOfWork.Save();
            return invoice;
        }

        public void Delete(Invoice invoice)
        {
            _unitOfWork.InvoiceRepository.Delete(invoice);
            _unitOfWork.Save();
        }

        public Invoice Create(Invoice invoice)
        {
            _unitOfWork.InvoiceRepository.Insert(invoice);
            _unitOfWork.Save();
            return invoice;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Invoice> GetByQuery()
        {
            return _unitOfWork.InvoiceRepository.GetByQuery();
        }
    }
}
