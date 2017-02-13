using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class BatchService : IBatchService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Batch> GetAll()
        {
            return _unitOfWork.BatchRepository.Get().ToList();
        }

        public Batch GetById(int? id)
        {
            return _unitOfWork.BatchRepository.GetByID(id.Value);
        }

        public Batch Update(Batch batch)
        {
            _unitOfWork.BatchRepository.Update(batch);
            _unitOfWork.Save();
            return batch;
        }

        public void Delete(Batch batch)
        {
            _unitOfWork.BatchRepository.Delete(batch);
            _unitOfWork.Save();
        }

        public Batch Create(Batch batch)
        {
            _unitOfWork.BatchRepository.Insert(batch);
            _unitOfWork.Save();
            return batch;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Batch> GetByQuery()
        {
            return _unitOfWork.BatchRepository.GetByQuery();
        }
    }
}
