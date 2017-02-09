using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Unosquare.Swan.AspNetCore.Sample.Database;

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
        
        [HttpPut("{id}")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);

            product.Name = "Coca";

            _context.SaveChanges();
            return  Ok(product);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return Ok(product);
        }
    }
}
