using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class PurchaseOrderItemService : IPurchaseOrderItemService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<PurchaseOrderItem> GetAll()
        {
            return _unitOfWork.PurchaseOrderItemRepository.Get().ToList();
        }

        public PurchaseOrderItem GetById(int? id)
        {
            return _unitOfWork.PurchaseOrderItemRepository.GetByID(id.Value);
        }

        public PurchaseOrderItem Update(PurchaseOrderItem purchaseOrderItem)
        {
            _unitOfWork.PurchaseOrderItemRepository.Update(purchaseOrderItem);
            _unitOfWork.Save();
            return purchaseOrderItem;
        }

        public void Delete(PurchaseOrderItem purchaseOrderItem)
        {
            _unitOfWork.PurchaseOrderItemRepository.Delete(purchaseOrderItem);
            _unitOfWork.Save();
        }

        public PurchaseOrderItem Create(PurchaseOrderItem purchaseOrderItem)
        {
            _unitOfWork.PurchaseOrderItemRepository.Insert(purchaseOrderItem);
            _unitOfWork.Save();
            return purchaseOrderItem;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<PurchaseOrderItem> GetByQuery()
        {
            return _unitOfWork.PurchaseOrderItemRepository.GetByQuery();
        }
    }
}
