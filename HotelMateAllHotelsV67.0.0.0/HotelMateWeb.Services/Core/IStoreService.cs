using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IStoreService
    {
        IList<Store> GetAll();
        Store GetById(int? id);
        Store Update(Store Store);
        void Delete(Store Store);
        Store Create(Store Store);
        IQueryable<Store> GetByQuery();
    }

}
