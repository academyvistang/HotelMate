using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Helpers
{
    public class MySingleton
    {
        private static volatile MySingleton instance = null;
        private static object syncRoot = new object();
        public  Dictionary<string, string> contextDictionary = new Dictionary<string, string>();

        /// <summary>
        /// The instance of the singleton
        /// safe for multithreading
        /// </summary>
        public static MySingleton Instance
        {
            get
            {
                // only create a new instance if one doesn't already exist.
                if (instance == null)
                {
                    // use this lock to ensure that only one thread can access
                    // this block of code at once.
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {                            
                            instance = new MySingleton();
                        }
                    }
                }
                // return instance where it was just created or already existed.
                return instance;
            }
        }


        /// <summary>
        /// This constructor must be kept private
        /// only access the singleton through the static Instance property
        /// </summary>
        private MySingleton()
        {

        }
    }
}