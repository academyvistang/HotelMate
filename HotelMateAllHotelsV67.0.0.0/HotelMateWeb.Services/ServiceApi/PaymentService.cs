using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class PaymentService : IPaymentService
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Payment> GetAllEvery()
        {
            return _unitOfWork.PaymentRepository.Get().Where(x => x.IsActive).ToList();
        }

        public IList<Payment> GetAllIncludeHotel(string include)
        {
            return _unitOfWork.PaymentRepository.GetByQuery(null, null, include).Where(x => x.IsActive && x.Type == 2).ToList();
        }

        public IList<Payment> GetAllInclude(string include)
        {
            return _unitOfWork.PaymentRepository.GetByQuery(null, null, include).Where(x => x.IsActive && x.Type == 1).ToList();
        }

        public IList<Payment> GetAllHotel()
        {
            return _unitOfWork.PaymentRepository.Get().Where(x => x.IsActive && x.Type == 2).ToList();
        }

        public IList<Payment> GetAll()
        {
            return _unitOfWork.PaymentRepository.Get().Where(x => x.IsActive && x.Type == 1).ToList();
        }

        public Payment GetById(int? id)
        {
            return _unitOfWork.PaymentRepository.GetByID(id.Value);
        }

        public Payment Update(Payment Payment)
        {
            _unitOfWork.PaymentRepository.Update(Payment);
            _unitOfWork.Save();
            return Payment;
        }

        public void Delete(Payment Payment)
        {
            _unitOfWork.PaymentRepository.Delete(Payment);
            _unitOfWork.Save();
        }

        public Payment Create(Payment Payment)
        {
            _unitOfWork.PaymentRepository.Insert(Payment);
            _unitOfWork.Save();
            return Payment;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_unitOfWork != null)
                {
                    _unitOfWork.Dispose();
                    _unitOfWork = null;
                }
            }
        }


        public IQueryable<Payment> GetByQuery()
        {
            return _unitOfWork.PaymentRepository.GetByQuery();
        }
    }
}
