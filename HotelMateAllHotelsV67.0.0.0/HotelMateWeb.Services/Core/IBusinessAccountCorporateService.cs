using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
 
    public interface IBusinessAccountCorporateService
    {
        IList<BusinessCorporateAccount> GetAll(int hotelId);
        IQueryable<BusinessCorporateAccount> GetByQuery(int hotelId);
        BusinessCorporateAccount GetById(int? id);
        BusinessCorporateAccount Update(BusinessCorporateAccount businessCorporateAccount);
        void Delete(BusinessCorporateAccount businessCorporateAccount);
        BusinessCorporateAccount Create(BusinessCorporateAccount businessCorporateAccount);
        IQueryable<BusinessCorporateAccount> GetAllForCompanyByType(int? companyId, int? paymentmethodId);
    }
}

