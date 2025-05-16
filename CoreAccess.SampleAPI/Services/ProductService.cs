using System.Text.Json;
using CoreAccess.SampleAPI.Model;
using ExampleApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.SampleAPI.Services
{
    public class ProductService
    {
        private List<Product> _products;

        public ProductService()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./data/example-data.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Product>();
            }
            else
            {
                _products = new List<Product>();
            }
        }
        
        public Product Create(Product product)
        {
            product.Id = Guid.NewGuid();
            _products.Add(product);
            SaveChanges();
            return product;
        }

        public PagedResult<Product> GetAll(ProductSearchOptions options)
        {
            var query = _products.AsQueryable();

            if (!string.IsNullOrEmpty(options.Id.ToString()))
            {
                query = query.Where(p => p.Id.Equals(options.Id));
            }

            if (!string.IsNullOrEmpty(options.ProductName))
                query = query.Where(p => p.ProductName.Contains(options.ProductName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(options.Brand))
                query = query.Where(p => p.Brand.Contains(options.Brand, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(options.Color))
                query = query.Where(p => p.Color.Contains(options.Color, StringComparison.OrdinalIgnoreCase));

            if (options.MinPrice.HasValue)
                query = query.Where(p => (decimal)p.Price >= options.MinPrice.Value);

            if (options.MaxPrice.HasValue)
                query = query.Where(p => (decimal)p.Price <= options.MaxPrice.Value);
            
            var totalCount = query.Count();

            var items = query
                .OrderBy(p => p.ProductName)
                .Skip((options.Page - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToList();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = options.Page,
                PageSize = options.PageSize
            };
        }
        
        public Product Update(string id, Product updatedProduct)
        {
            var product = _products.FirstOrDefault(p => p.Id == Guid.Parse(id));
            if (product != null)
            {
                product.ProductName = updatedProduct.ProductName;
                product.Brand = updatedProduct.Brand;
                product.ModelNumber = updatedProduct.ModelNumber;
                product.Price = updatedProduct.Price;
                product.QuantityInStock = updatedProduct.QuantityInStock;
                product.Weight = updatedProduct.Weight;
                product.Dimensions = updatedProduct.Dimensions;
                product.Color = updatedProduct.Color;
                product.ReleaseDate = updatedProduct.ReleaseDate;
                product.WarrantyPeriod = updatedProduct.WarrantyPeriod;
                product.Manufacturer = updatedProduct.Manufacturer;
                product.CountryOfOrigin = updatedProduct.CountryOfOrigin;
                product.Description = updatedProduct.Description;
                product.Barcode = updatedProduct.Barcode;

                SaveChanges();
            }
            return product;
        }
        
        public bool Delete(string id)
        {
            var product = _products.FirstOrDefault(p => p.Id == Guid.Parse(id));
            if (product != null)
            {
                _products.Remove(product);
                SaveChanges();
                return true;
            }
            return false;
        }
        
        private void SaveChanges()
        {
            var json = JsonSerializer.Serialize(_products, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "example-data.json"), json);
        }
    }
}