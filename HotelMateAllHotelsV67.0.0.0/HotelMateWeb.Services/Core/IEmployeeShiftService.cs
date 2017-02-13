using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{
    public interface IEmployeeShiftService
    {
        IList<EmployeeShift> GetAll(int hotelId);
        EmployeeShift GetById(int? id);
        EmployeeShift Update(EmployeeShift employeeShift);
        void Delete(EmployeeShift employeeShift);
        EmployeeShift Create(EmployeeShift employeeShift);
        IQueryable<EmployeeShift> GetByQuery(int hotelId);
    }
}
