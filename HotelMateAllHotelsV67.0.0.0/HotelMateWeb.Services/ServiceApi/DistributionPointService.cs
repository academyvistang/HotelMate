using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class DistributionPointService : IDistributionPointService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<DistributionPoint> GetAll()
        {
            return _unitOfWork.DistributionPointRepository.Get().ToList();
        }

        public DistributionPoint GetById(int? id)
        {
            return _unitOfWork.DistributionPointRepository.GetByID(id.Value);
        }

        public DistributionPoint Update(DistributionPoint distributionPoint)
        {
            _unitOfWork.DistributionPointRepository.Update(distributionPoint);
            _unitOfWork.Save();
            return distributionPoint;
        }

        public void Delete(DistributionPoint distributionPoint)
        {
            _unitOfWork.DistributionPointRepository.Delete(distributionPoint);
            _unitOfWork.Save();
        }

        public DistributionPoint Create(DistributionPoint distributionPoint)
        {
            _unitOfWork.DistributionPointRepository.Insert(distributionPoint);
            _unitOfWork.Save();
            return distributionPoint;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<DistributionPoint> GetByQuery()
        {
            return _unitOfWork.DistributionPointRepository.GetByQuery();
        }
    }
}
