using System;
using System.Collections.Generic;

namespace ExampleApi.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string ModelNumber { get; set; }
        public double Price { get; set; }
        public int QuantityInStock { get; set; }
        public double Weight { get; set; }
        public string Dimensions { get; set; }
        public string Color { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int WarrantyPeriod { get; set; }
        public string Manufacturer { get; set; }
        public string CountryOfOrigin { get; set; }
        public string Description { get; set; }
        public long Barcode { get; set; }
    }
}