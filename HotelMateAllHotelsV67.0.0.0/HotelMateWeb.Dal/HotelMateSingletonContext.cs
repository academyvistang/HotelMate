using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Dal
{
    
    public sealed class HotelMateSingletonContext
    {
        private static readonly HotelMateWebEntities instance = new HotelMateWebEntities();

        static HotelMateSingletonContext() { }

        private HotelMateSingletonContext() { }

        public static HotelMateWebEntities Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
