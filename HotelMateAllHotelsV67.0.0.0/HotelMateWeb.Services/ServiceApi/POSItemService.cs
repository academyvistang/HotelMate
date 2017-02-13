using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class POSItemService : IPOSItemService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<POSItem> GetAllInclude(string includeProperties)
        {
            return _unitOfWork.POSItemRepository.GetByQuery(null, null, includeProperties).ToList();
        }

        public IList<POSItem> GetAll()
        {
            return _unitOfWork.POSItemRepository.Get(x => x.IsActive).ToList();
        }

        public POSItem GetById(int? id)
        {
            return _unitOfWork.POSItemRepository.GetByID(id.Value);
        }

        public POSItem Update(POSItem pOSItem)
        {
            _unitOfWork.POSItemRepository.Update(pOSItem);
            _unitOfWork.Save();
            return pOSItem;
        }

        public void Delete(POSItem pOSItem)
        {
            _unitOfWork.POSItemRepository.Delete(pOSItem);
            _unitOfWork.Save();
        }

        public POSItem Create(POSItem pOSItem)
        {
            _unitOfWork.POSItemRepository.Insert(pOSItem);
            _unitOfWork.Save();
            return pOSItem;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<POSItem> GetByQuery()
        {
            return _unitOfWork.POSItemRepository.GetByQuery();
        }
    }
}
