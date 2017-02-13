using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class SoldItemService : ISoldItemService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<SoldItemsAll> GetAll()
        {
            return _unitOfWork.SoldItemRepository.Get().ToList();
        }

        public SoldItemsAll GetById(int? id)
        {
            return _unitOfWork.SoldItemRepository.GetByID(id.Value);
        }

        public SoldItemsAll Update(SoldItemsAll soldItem)
        {
            _unitOfWork.SoldItemRepository.Update(soldItem);
            _unitOfWork.Save();
            return soldItem;
        }

        public void Delete(SoldItemsAll soldItem)
        {
            _unitOfWork.SoldItemRepository.Delete(soldItem);
            _unitOfWork.Save();
        }

        public SoldItemsAll Create(SoldItemsAll soldItem)
        {
            _unitOfWork.SoldItemRepository.Insert(soldItem);
            _unitOfWork.Save();
            return soldItem;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<SoldItemsAll> GetByQuery()
        {
            return _unitOfWork.SoldItemRepository.GetByQuery();
        }
    }
}
