using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class PaymentMethodService : IPaymentMethodService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<PaymentMethod> GetAll(int hotelId)
        {
            return _unitOfWork.PaymentMethodRepository.Get().ToList();
        }

        public PaymentMethod GetById(int? id)
        {
            return _unitOfWork.PaymentMethodRepository.GetByID(id.Value);
        }

        public PaymentMethod Update(PaymentMethod paymentMethod)
        {
            _unitOfWork.PaymentMethodRepository.Update(paymentMethod);
            _unitOfWork.Save();
            return paymentMethod;
        }

        public void Delete(PaymentMethod paymentMethod)
        {
            _unitOfWork.PaymentMethodRepository.Delete(paymentMethod);
            _unitOfWork.Save();
        }

        public PaymentMethod Create(PaymentMethod paymentMethod)
        {
            _unitOfWork.PaymentMethodRepository.Insert(paymentMethod);
            _unitOfWork.Save();
            return paymentMethod;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        public IQueryable<PaymentMethod> GetByQuery(int hotelId)
        {
            return _unitOfWork.PaymentMethodRepository.GetByQuery();
        }
    }
}
