using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IPurchaseOrderService
    {
        IList<PurchaseOrder> GetAll();
        IList<PurchaseOrder> GetAll(string includes);
        PurchaseOrder GetById(int? id);
        PurchaseOrder Update(PurchaseOrder PurchaseOrder);
        void Delete(PurchaseOrder PurchaseOrder);
        PurchaseOrder Create(PurchaseOrder PurchaseOrder);
        IQueryable<PurchaseOrder> GetByQuery();
    }

}
