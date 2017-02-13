using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IBatchService
    {
        IList<Batch> GetAll();
        Batch GetById(int? id);
        Batch Update(Batch Batch);
        void Delete(Batch Batch);
        Batch Create(Batch Batch);
        IQueryable<Batch> GetByQuery();
    }

}
