using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IStoreItemService
    {
        IList<StoreItem> GetAll();
        StoreItem GetById(int? id);
        StoreItem Update(StoreItem StoreItem);
        void Delete(StoreItem StoreItem);
        StoreItem Create(StoreItem StoreItem);
        IQueryable<StoreItem> GetByQuery();
    }

}
