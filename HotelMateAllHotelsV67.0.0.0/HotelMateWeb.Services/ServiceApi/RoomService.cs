using HotelMateWeb.Dal;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HotelMateWeb.Services.ServiceApi
{
    public class RoomService : IRoomService, IDisposable
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public IList<Room> GetAll(int hotelId)
        {
            return _unitOfWork.RoomRepository.Get(x => x.HotelId == hotelId).ToList();
        }

        public Room GetById(int? id)
        {
            return _unitOfWork.RoomRepository.GetByID(id.Value);
        }

        public Room Update(Room room)
        {
            try
            {
                _unitOfWork.RoomRepository.Update(room);
                _unitOfWork.Save();
                return room;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(Room room)
        {
            
            room = _unitOfWork.RoomRepository.GetByQuery(x => x.Id == room.Id, null, "Picturals").FirstOrDefault();

            var pics = room.Picturals.ToList();

            foreach(var p in pics)
            {
                _unitOfWork.PicturalRepository.Delete(p);
            }
          
            _unitOfWork.RoomRepository.Delete(room);
            _unitOfWork.Save();
        }

        public Room Create(Room room)
        {
            _unitOfWork.RoomRepository.Insert(room);
            _unitOfWork.Save();
            return room;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public IQueryable<Room> GetByQuery(int hotelId)
        {
            return _unitOfWork.RoomRepository.GetByQuery(x => x.HotelId == hotelId);
        }
    }
}
