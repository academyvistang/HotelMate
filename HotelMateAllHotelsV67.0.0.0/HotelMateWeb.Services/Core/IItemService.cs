using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IItemService
    {
        IList<Item> GetAll();
        Item GetById(int? id);
        Item Update(Item item);
        void Delete(Item item);
        Item Create(Item item);
        IQueryable<Item> GetByQuery();
    }

}
