using System.Text.Json;
using ExampleApi.Models;

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

        // CREATE
        public Product Create(Product product)
        {
            product.Id = Guid.NewGuid(); // Automatisch eine neue ID zuweisen
            _products.Add(product);
            SaveChanges();
            return product;
        }

        // READ (get all products with optional search filters)
        public IEnumerable<Product> GetAll(SearchOptions searchOptions)
        {
            var query = _products.AsQueryable();

            if (!string.IsNullOrEmpty(searchOptions.ProductName))
                query = query.Where(p => p.ProductName.Contains(searchOptions.ProductName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(searchOptions.Brand))
                query = query.Where(p => p.Brand.Contains(searchOptions.Brand, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(searchOptions.Color))
                query = query.Where(p => p.Color.Contains(searchOptions.Color, StringComparison.OrdinalIgnoreCase));

            if (searchOptions.MinPrice.HasValue)
                query = query.Where(p => p.Price >= searchOptions.MinPrice.Value);

            if (searchOptions.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= searchOptions.MaxPrice.Value);

            return query.ToList();
        }

        // READ (get a single product by ID)
        public Product GetById(string id)
        {
            return _products.FirstOrDefault(p => p.Id == Guid.Parse(id));
        }

        // UPDATE
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

        // DELETE
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

        // Helper Method to save changes to the JSON file
        private void SaveChanges()
        {
            var json = JsonSerializer.Serialize(_products, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "example-data.json"), json);
        }
    }

    // Optional Search Options object
    public class SearchOptions
    {
        public string? ProductName { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
    }
}