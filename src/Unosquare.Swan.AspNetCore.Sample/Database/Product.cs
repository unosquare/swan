using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.AspNetCore.Sample.Database
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        public string Name { get; set; }
    }
}
