using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
    public interface IPaymentMethodService
    {
        IList<PaymentMethod> GetAll(int hotelId);
        PaymentMethod GetById(int? id);
        PaymentMethod Update(PaymentMethod paymentMethod);
        void Delete(PaymentMethod paymentMethod);
        PaymentMethod Create(PaymentMethod paymentMethod);
        IQueryable<PaymentMethod> GetByQuery(int hotelId);
    }
}
