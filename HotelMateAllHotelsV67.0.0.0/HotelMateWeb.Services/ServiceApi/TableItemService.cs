using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class TableItemService : ITableItemService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<TableItem> GetAll()
        {
            return _unitOfWork.TableItemRepository.Get().ToList();
        }

        public TableItem GetById(int? id)
        {
            return _unitOfWork.TableItemRepository.GetByID(id.Value);
        }

        public TableItem Update(TableItem tableItem)
        {
            _unitOfWork.TableItemRepository.Update(tableItem);
            _unitOfWork.Save();
            return tableItem;
        }

        public void Delete(TableItem tableItem)
        {
            _unitOfWork.TableItemRepository.Delete(tableItem);
            _unitOfWork.Save();
        }

        public TableItem Create(TableItem tableItem)
        {
            _unitOfWork.TableItemRepository.Insert(tableItem);
            _unitOfWork.Save();
            return tableItem;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<TableItem> GetByQuery()
        {
            return _unitOfWork.TableItemRepository.GetByQuery();
        }
    }
}
