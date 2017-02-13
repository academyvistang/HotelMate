using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Services.Core
{
    public interface IPersonTypeService
    {
        IList<PersonType> GetAll(int hotelId);
        PersonType GetById(int? id);
        PersonType Update(PersonType personType);
        void Delete(PersonType personType);
        PersonType Create(PersonType personType);
        IQueryable<PersonType> GetByQuery(int hotelId);       
        
    }
}
