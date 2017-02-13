using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class TransactionService : ITransactionService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Transaction> GetAll(int hotelId)
        {
            return _unitOfWork.TransactionRepository.Get(x => x.Guest.HotelId == hotelId).ToList();
        }

        public Transaction GetById(int? id)
        {
            return _unitOfWork.TransactionRepository.GetByID(id.Value);
        }

        public Transaction Update(Transaction transaction)
        {
            _unitOfWork.TransactionRepository.Update(transaction);
            _unitOfWork.Save();
            return transaction;
        }

        public void Delete(Transaction transaction)
        {
            _unitOfWork.TransactionRepository.Delete(transaction);
            _unitOfWork.Save();
        }

        public Transaction Create(Transaction transaction)
        {
            _unitOfWork.TransactionRepository.Insert(transaction);
            _unitOfWork.Save();
            return transaction;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Transaction> GetByQuery(int hotelId)
        {
            return _unitOfWork.TransactionRepository.GetByQuery(x => x.Guest.HotelId == hotelId);
        }
    }
}
