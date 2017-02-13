using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IDistributionPointService
    {
        IList<DistributionPoint> GetAll();
        DistributionPoint GetById(int? id);
        DistributionPoint Update(DistributionPoint DistributionPoint);
        void Delete(DistributionPoint DistributionPoint);
        DistributionPoint Create(DistributionPoint DistributionPoint);
        IQueryable<DistributionPoint> GetByQuery();
    }

}
