using CatalogService.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace CatalogService.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("OnlineCatalogDB");
            _products = database.GetCollection<Product>("Products");
        }

        public List<Product> Get() => _products.Find(p => true).ToList();

        public Product Get(string id) => _products.Find(p => p.Id == id).FirstOrDefault();

        public Product Create(Product product)
        {
            _products.InsertOne(product);
            return product;
        }

        public void Update(string id, Product product) =>
            _products.ReplaceOne(p => p.Id == id, product);

        public void Remove(string id) => _products.DeleteOne(p => p.Id == id);
    }
}
