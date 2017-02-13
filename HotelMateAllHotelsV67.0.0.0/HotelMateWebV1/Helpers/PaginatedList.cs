using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelMateWebV1.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }


        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public int? GuestId { get; private set; }
        public int? RoomId { get; private set; }
        public int? PaymentTypeId { get; private set; }


        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize, int? guestId, int? roomId, int? paymentTypeId )
        {
            GuestId = guestId;
            RoomId = roomId;
            PaymentTypeId = paymentTypeId;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            AddRange(source.Skip(PageIndex * PageSize).Take(PageSize));
        }

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            AddRange(source.Skip(PageIndex * PageSize).Take(PageSize));
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex + 1 < TotalPages);
            }
        }
    }
}