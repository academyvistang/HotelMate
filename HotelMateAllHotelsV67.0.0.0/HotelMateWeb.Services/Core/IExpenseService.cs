using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IExpenseService
    {
        IList<Expense> GetAll();
        Expense GetById(int? id);
        Expense Update(Expense Expense);
        void Delete(Expense Expense);
        Expense Create(Expense Expense);
        IQueryable<Expense> GetByQuery();
    }

}
