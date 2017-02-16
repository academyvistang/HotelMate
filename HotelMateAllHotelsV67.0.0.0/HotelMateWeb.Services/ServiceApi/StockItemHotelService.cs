using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class StockItemHotelService : IStockItemHotelService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<StockItemHotel> GetAll()
        {
            return _unitOfWork.StockItemHotelRepository.Get().ToList();
        }

        public StockItemHotel GetById(int? id)
        {
            return _unitOfWork.StockItemHotelRepository.GetByID(id.Value);
        }

        public StockItemHotel Update(StockItemHotel StockItemHotel)
        {
            _unitOfWork.StockItemHotelRepository.Update(StockItemHotel);
            _unitOfWork.Save();
            return StockItemHotel;
        }

        public void Delete(StockItemHotel StockItemHotel)
        {
            _unitOfWork.StockItemHotelRepository.Delete(StockItemHotel);
            _unitOfWork.Save();
        }

        public StockItemHotel Create(StockItemHotel StockItemHotel)
        {
            _unitOfWork.StockItemHotelRepository.Insert(StockItemHotel);
            _unitOfWork.Save();
            return StockItemHotel;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<StockItemHotel> GetByQuery()
        {
            return _unitOfWork.StockItemHotelRepository.GetByQuery();
        }
    }
}
