using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Unosquare.Swan.AspNetCore.Sample.Database;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Unosquare.Swan.AspNetCore.Sample.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly SampleDbContext _context;

        public ProductController(SampleDbContext context)
        {
            _context = context;
        }


        // GET: api/values
        [HttpGet]
        public IActionResult Get()
        {
            _context.Products.AddRange(
                new[]
                {
                    new Product { Name = "Gatorade" },
                    new Product { Name = "Red Bull"},
                    new Product { Name = "Powerade"},
                    new Product { Name = "Electrolit" }
                });

            _context.SaveChanges();
            return Ok();
        }

        // GET api/values/5
        [HttpPut("{id}")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);

            product.Name = "Coca";

            _context.SaveChanges();
            return  Ok();
        }
    }
}
