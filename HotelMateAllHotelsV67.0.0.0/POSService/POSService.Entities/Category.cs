using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSService.Entities
{
    public class Category
    {
        // Properties
        public string Description { get; set; }

        public int Id { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }

    }
}
