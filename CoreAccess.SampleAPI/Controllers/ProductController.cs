using CoreAccess.SampleAPI.Services;
using ExampleApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.SampleAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class ProductController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] SearchOptions searchOptions)
    {
        var products = productService.GetAll(searchOptions);
        return Ok(products);
    }
    
    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        if (product == null)
            return BadRequest("Product cannot be null.");

        var createdProduct = productService.Create(product);
        return Ok(createdProduct);
    }
    
    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Product product)
    {
        if (product == null)
            return BadRequest("Product cannot be null.");

        var updatedProduct = productService.Update(id, product);
        if (updatedProduct == null)
            return NotFound();

        return Ok(updatedProduct);
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var deletedProduct = productService.Delete(id);
        if (deletedProduct == null)
            return NotFound();

        return Ok(deletedProduct);
    }
}