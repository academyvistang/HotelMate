using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{

    public class MovieCategoryService : IMovieCategoryService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<MovieCategory> GetAll()
        {
            return _unitOfWork.MovieCategoryRepository.Get().ToList();
        }

        public MovieCategory GetById(int? id)
        {
            return _unitOfWork.MovieCategoryRepository.GetByID(id.Value);
        }

        public MovieCategory Update(MovieCategory movieCategory)
        {
            _unitOfWork.MovieCategoryRepository.Update(movieCategory);
            _unitOfWork.Save();
            return movieCategory;
        }

        public void Delete(MovieCategory movieCategory)
        {
            _unitOfWork.MovieCategoryRepository.Delete(movieCategory);
            _unitOfWork.Save();
        }

        public MovieCategory Create(MovieCategory movieCategory)
        {
            _unitOfWork.MovieCategoryRepository.Insert(movieCategory);
            _unitOfWork.Save();
            return movieCategory;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<MovieCategory> GetByQuery()
        {
            return _unitOfWork.MovieCategoryRepository.GetByQuery();
        }
    }
}
