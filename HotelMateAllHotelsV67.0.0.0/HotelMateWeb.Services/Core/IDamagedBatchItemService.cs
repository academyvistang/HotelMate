using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IDamagedBatchItemService
    {
        IList<DamagedBatchItem> GetAll();
        DamagedBatchItem GetById(int? id);
        DamagedBatchItem Update(DamagedBatchItem DamagedBatchItem);
        void Delete(DamagedBatchItem DamagedBatchItem);
        DamagedBatchItem Create(DamagedBatchItem DamagedBatchItem);
        IQueryable<DamagedBatchItem> GetByQuery();
    }

}
