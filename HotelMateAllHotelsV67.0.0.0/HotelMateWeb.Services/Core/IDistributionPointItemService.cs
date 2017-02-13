using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IDistributionPointItemService
    {
        IList<DistributionPointItem> GetAll();
        DistributionPointItem GetById(int? id);
        DistributionPointItem Update(DistributionPointItem DistributionPointItem);
        void Delete(DistributionPointItem DistributionPointItem);
        DistributionPointItem Create(DistributionPointItem DistributionPointItem);
        IQueryable<DistributionPointItem> GetByQuery();
    }

}
