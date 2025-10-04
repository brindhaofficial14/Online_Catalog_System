using CatalogService.Models;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;
        public ProductsController(ProductService service) { _service = service; }

        [HttpGet]
        [Authorize(Roles = "Customer,Vendor,Admin,IT")]
        public ActionResult<List<Product>> Get() => _service.Get();

        [HttpGet("{id:length(24)}")]
        [Authorize(Roles = "Customer,Vendor,Admin,IT")]
        public ActionResult<Product> Get(string id) { var product = _service.Get(id); if (product == null) return NotFound(); return product; }

        [HttpPost]
        [Authorize(Roles = "Vendor,Admin")]
        public ActionResult<Product> Create(Product product) { _service.Create(product); return CreatedAtAction(nameof(Get), new { id = product.Id }, product); }

        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Vendor,Admin")]
        public IActionResult Update(string id, Product product) { var existing = _service.Get(id); if (existing == null) return NotFound(); _service.Update(id, product); return NoContent(); }

        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(string id) { var product = _service.Get(id); if (product == null) return NotFound(); _service.Remove(id); return NoContent(); }
    }
}
