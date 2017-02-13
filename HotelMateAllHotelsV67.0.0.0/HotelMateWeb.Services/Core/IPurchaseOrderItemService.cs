using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IPurchaseOrderItemService
    {
        IList<PurchaseOrderItem> GetAll();
        PurchaseOrderItem GetById(int? id);
        PurchaseOrderItem Update(PurchaseOrderItem PurchaseOrderItem);
        void Delete(PurchaseOrderItem PurchaseOrderItem);
        PurchaseOrderItem Create(PurchaseOrderItem PurchaseOrderItem);
        IQueryable<PurchaseOrderItem> GetByQuery();
    }

}
