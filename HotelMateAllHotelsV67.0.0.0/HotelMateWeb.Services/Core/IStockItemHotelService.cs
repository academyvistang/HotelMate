using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IStockItemHotelService
    {
        IList<StockItemHotel> GetAll();
        StockItemHotel GetById(int? id);
        StockItemHotel Update(StockItemHotel StockItemHotel);
        void Delete(StockItemHotel StockItemHotel);
        StockItemHotel Create(StockItemHotel StockItemHotel);
        IQueryable<StockItemHotel> GetByQuery();
    }

}
