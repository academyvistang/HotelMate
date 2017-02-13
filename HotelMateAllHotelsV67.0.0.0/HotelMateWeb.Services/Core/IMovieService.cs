using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelMateWeb.Dal.DataCore;

namespace HotelMateWeb.Services.Core
{

    public interface IMovieService
    {
        IList<Movie> GetAll();
        Movie GetById(int? id);
        Movie Update(Movie Movie);
        void Delete(Movie Movie);
        Movie Create(Movie Movie);
        IQueryable<Movie> GetByQuery();
    }

}
