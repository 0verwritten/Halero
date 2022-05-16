using System.Linq.Expressions;

namespace Halero.Services;

interface IDatabaseManager<T>{
    void InsertOne(T elem);
    T? FindOne( Expression<Func<T, bool>> predicate );
    void UpdateOne( Expression<Func<T, bool>> predicate, T value );
    void DeleteOne( Expression<Func<T, bool>> predicate );
}