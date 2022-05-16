using MongoDB.Driver;
using MongoDB.Bson;
using Halero.Models;

namespace Halero.Services;

public class MongoDBSessionManager{
    private readonly IMongoDatabase mongoDatabase;

    public MongoDBSessionManager(DatabaseConfigModel databaseConfig){
        mongoDatabase = new MongoClient(databaseConfig.ConnectionStirng)
                            .GetDatabase(databaseConfig.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName) => mongoDatabase.GetCollection<T>(collectionName);
}