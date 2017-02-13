using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class MovieService : IMovieService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Movie> GetAll()
        {
            return _unitOfWork.MovieRepository.Get().ToList();
        }

        public Movie GetById(int? id)
        {
            return _unitOfWork.MovieRepository.GetByID(id.Value);
        }

        public Movie Update(Movie movie)
        {
            _unitOfWork.MovieRepository.Update(movie);
            _unitOfWork.Save();
            return movie;
        }

        public void Delete(Movie movie)
        {
            _unitOfWork.MovieRepository.Delete(movie);
            _unitOfWork.Save();
        }

        public Movie Create(Movie movie)
        {
            _unitOfWork.MovieRepository.Insert(movie);
            _unitOfWork.Save();
            return movie;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Movie> GetByQuery()
        {
            return _unitOfWork.MovieRepository.GetByQuery();
        }
    }
}
