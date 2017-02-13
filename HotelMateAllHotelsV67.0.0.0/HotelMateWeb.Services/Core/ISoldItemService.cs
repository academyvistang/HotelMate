using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface ISoldItemService
    {
        IList<SoldItemsAll> GetAll();
        SoldItemsAll GetById(int? id);
        SoldItemsAll Update(SoldItemsAll SoldItem);
        void Delete(SoldItemsAll SoldItem);
        SoldItemsAll Create(SoldItemsAll SoldItem);
        IQueryable<SoldItemsAll> GetByQuery();
    }

}
