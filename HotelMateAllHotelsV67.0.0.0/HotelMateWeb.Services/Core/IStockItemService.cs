using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IStockItemService
    {
        IList<StockItem> GetAll();
        StockItem GetById(int? id);
        StockItem Update(StockItem StockItem);
        void Delete(StockItem StockItem);
        StockItem Create(StockItem StockItem);
        IQueryable<StockItem> GetByQuery();
    }

}
