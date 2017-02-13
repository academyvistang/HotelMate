using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface ITableItemService
    {
        IList<TableItem> GetAll();
        TableItem GetById(int? id);
        TableItem Update(TableItem TableItem);
        void Delete(TableItem TableItem);
        TableItem Create(TableItem TableItem);
        IQueryable<TableItem> GetByQuery();
    }

}
