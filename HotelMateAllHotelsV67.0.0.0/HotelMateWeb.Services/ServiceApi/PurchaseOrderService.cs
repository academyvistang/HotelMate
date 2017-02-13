using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class PurchaseOrderService : IPurchaseOrderService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<PurchaseOrder> GetAll()
        {
            return _unitOfWork.PurchaseOrderRepository.Get(x => !x.Completed).ToList();
        }

        public IList<PurchaseOrder> GetAll(string includes)
        {
            return _unitOfWork.PurchaseOrderRepository.Get().ToList();
        }

        public PurchaseOrder GetById(int? id)
        {
            return _unitOfWork.PurchaseOrderRepository.GetByID(id.Value);
        }

        public PurchaseOrder Update(PurchaseOrder purchaseOrder)
        {
            _unitOfWork.PurchaseOrderRepository.Update(purchaseOrder);
            _unitOfWork.Save();
            return purchaseOrder;
        }

        public void Delete(PurchaseOrder purchaseOrder)
        {
            _unitOfWork.PurchaseOrderRepository.Delete(purchaseOrder);
            _unitOfWork.Save();
        }

        public PurchaseOrder Create(PurchaseOrder purchaseOrder)
        {
            _unitOfWork.PurchaseOrderRepository.Insert(purchaseOrder);
            _unitOfWork.Save();
            return purchaseOrder;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<PurchaseOrder> GetByQuery()
        {
            return _unitOfWork.PurchaseOrderRepository.GetByQuery();
        }
    }
}
