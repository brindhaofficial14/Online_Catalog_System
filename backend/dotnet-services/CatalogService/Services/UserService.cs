using CatalogService.Models;
using MongoDB.Driver;

namespace CatalogService.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("OnlineCatalogDB");
            _users = database.GetCollection<User>("Users");
        }

        public User GetByUsername(string username) =>
            _users.Find(u => u.Username == username).FirstOrDefault();

        public User Create(User user)
        {
            _users.InsertOne(user);
            return user;
        }
    }
}
