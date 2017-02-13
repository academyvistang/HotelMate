using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class InvoiceStatusTypeService : IInvoiceStatusTypeService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<InvoiceStatusType> GetAll()
        {
            return _unitOfWork.InvoiceStatusTypeRepository.Get().ToList();
        }

        public InvoiceStatusType GetById(int? id)
        {
            return _unitOfWork.InvoiceStatusTypeRepository.GetByID(id.Value);
        }

        public InvoiceStatusType Update(InvoiceStatusType invoiceStatusType)
        {
            _unitOfWork.InvoiceStatusTypeRepository.Update(invoiceStatusType);
            _unitOfWork.Save();
            return invoiceStatusType;
        }

        public void Delete(InvoiceStatusType invoiceStatusType)
        {
            _unitOfWork.InvoiceStatusTypeRepository.Delete(invoiceStatusType);
            _unitOfWork.Save();
        }

        public InvoiceStatusType Create(InvoiceStatusType invoiceStatusType)
        {
            _unitOfWork.InvoiceStatusTypeRepository.Insert(invoiceStatusType);
            _unitOfWork.Save();
            return invoiceStatusType;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<InvoiceStatusType> GetByQuery()
        {
            return _unitOfWork.InvoiceStatusTypeRepository.GetByQuery();
        }
    }
}
