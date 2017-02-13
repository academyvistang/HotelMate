using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entitities;
using System.Data.Entity;

namespace HotelMateWebV1.Helpers
{
    public class Agbo21Initialiser : DropCreateDatabaseIfModelChanges<Agbo21Context>
    {
        protected override void Seed(Agbo21Context context)
        {
            base.Seed(context);
        }
    }
}