
using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Helpers
{



    public class GuestComparer : IEqualityComparer<Guest>
    {
        public GuestComparer()
        {

        }

        #region IComparer Members

        public bool Equals(Guest x, Guest y)
        {
            if (x.Telephone == y.Telephone)
            {
                return true;
            }
            else { return false; }
        }
        public int GetHashCode(Guest codeh)
        {
            return 0;
        }

        //public int Compare(ElectiveCourse tag1, ElectiveCourse tag2)
        //{
        //    return tag2.CourseId.CompareTo(tag1.CourseId);
        //}

        #endregion

    }
}