using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IMovieCategoryService
    {
        IList<MovieCategory> GetAll();
        MovieCategory GetById(int? id);
        MovieCategory Update(MovieCategory MovieCategory);
        void Delete(MovieCategory MovieCategory);
        MovieCategory Create(MovieCategory MovieCategory);
        IQueryable<MovieCategory> GetByQuery();
    }

}
