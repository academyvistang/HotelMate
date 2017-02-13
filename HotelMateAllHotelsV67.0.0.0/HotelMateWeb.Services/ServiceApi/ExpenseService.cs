using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class ExpenseService : IExpenseService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Expense> GetAll()
        {
            return _unitOfWork.ExpenseRepository.Get().ToList();
        }

        public Expense GetById(int? id)
        {
            return _unitOfWork.ExpenseRepository.GetByID(id.Value);
        }

        public Expense Update(Expense expense)
        {
            _unitOfWork.ExpenseRepository.Update(expense);
            _unitOfWork.Save();
            return expense;
        }

        public void Delete(Expense expense)
        {
            _unitOfWork.ExpenseRepository.Delete(expense);
            _unitOfWork.Save();
        }

        public Expense Create(Expense expense)
        {
            _unitOfWork.ExpenseRepository.Insert(expense);
            _unitOfWork.Save();
            return expense;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Expense> GetByQuery()
        {
            return _unitOfWork.ExpenseRepository.GetByQuery();
        }
    }
}
