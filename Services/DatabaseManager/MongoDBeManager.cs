using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using Halero.Models;

namespace Halero.Services;

public class MongoDBManager<T> :  IDatabaseManager<T>{
    private readonly IMongoCollection<T> collectionBase;

    public MongoDBManager(MongoDBSessionManager sessionManager, string collectionName) {        
        collectionBase = sessionManager.GetCollection<T>(collectionName);
    }

    public void InsertOne(T elem) => collectionBase.InsertOne(elem);
    
    public T? FindOne( Expression<Func<T, bool>> predicate ) => collectionBase.Find(predicate).FirstOrDefault<T>();

    public void DeleteOne( Expression<Func<T, bool>> predicate ) => collectionBase.DeleteOne(predicate);

    public void UpdateOne( Expression<Func<T, bool>> predicate, T value ) {
        var filter = Builders<T>.Filter.Where( predicate );
        collectionBase.ReplaceOne(filter, value);
    }

}