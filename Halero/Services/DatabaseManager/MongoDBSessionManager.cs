using MongoDB.Driver;
using MongoDB.Bson;
using Halero.Models;
using Microsoft.Extensions.Options;

namespace Halero.Services;

public class MongoDBSessionManager{
    private readonly IMongoDatabase mongoDatabase;

    public MongoDBSessionManager(IOptions<DatabaseConfigModel> databaseConfig){
        mongoDatabase = new MongoClient(databaseConfig.Value.ConnectionStirng)
                            .GetDatabase(databaseConfig.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName) => mongoDatabase.GetCollection<T>(collectionName);
}