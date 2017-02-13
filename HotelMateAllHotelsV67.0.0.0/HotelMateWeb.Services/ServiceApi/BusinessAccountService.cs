using System;
using System.Collections.Generic;
using System.Linq;
using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;

namespace HotelMateWeb.Services.ServiceApi
{
    public class BusinessAccountService : IBusinessAccountService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();


        public BusinessAccount Update(BusinessAccount company, List<GuestRoom> accountRooms)
        {
            var giIds = accountRooms.Select(x => x.Id).ToList();

            var all = _unitOfWork.GuestRoomRepository.Get().Where(x => giIds.Contains(x.Id)).ToList();

            foreach(var a in all)
            {
                company.GuestRooms.Add(a);
            }

            _unitOfWork.BusinessAccountRepository.Update(company);
            _unitOfWork.Save();
            return company;


        }

        public IList<BusinessAccount> GetAll(int hotelId)
        {
            return _unitOfWork.BusinessAccountRepository.Get(x => x.HotelId == hotelId).ToList();
        }

        public BusinessAccount GetById(int? id)
        {
            return _unitOfWork.BusinessAccountRepository.GetByID(id.Value);
        }

        public BusinessAccount Update(BusinessAccount businessAccount)
        {
            _unitOfWork.BusinessAccountRepository.Update(businessAccount);
            _unitOfWork.Save();
            return businessAccount;
        }

        public void Delete(BusinessAccount businessAccount)
        {
            _unitOfWork.BusinessAccountRepository.Delete(businessAccount);
            _unitOfWork.Save();
        }

        public BusinessAccount Create(BusinessAccount businessAccount)
        {
            _unitOfWork.BusinessAccountRepository.Insert(businessAccount);
            _unitOfWork.Save();
            return businessAccount;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<BusinessAccount> GetByQuery(int hotelId)
        {
            return _unitOfWork.BusinessAccountRepository.GetByQuery(x => x.HotelId == hotelId);
        }
    }
}
