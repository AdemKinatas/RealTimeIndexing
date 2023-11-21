using System.Linq.Expressions;

namespace RealTimeIndexing.Services.ElasticSearch
{
    public interface IElasticsearchService<T> where T : class, new()
    {
        Task CheckIndex(string indexName);
        Task DeleteIndex(string indexName);
        Task<T> GetByIdAsync(int id, string indexName);
        Task<T> GetByFieldAsync<TField>(Expression<Func<T, TField>> field, TField value, string indexName);
        Task<IEnumerable<T>> GetAllAsync(string indexName);
        Task AddManyAsync(IEnumerable<T> entities, string indexName);
        Task AddMultipleByDocumentIdAsync(IEnumerable<T> entities, string indexName);
        Task AddAsync(T entity, string indexName);
        Task AddByDocumentIdAsync(T entity, string indexName);
        Task UpdateAsync(int id, T entity, string indexName);
        Task UpdateByFieldAsync<TField>(TField value, T entity, string indexName, Expression<Func<T, TField>> field);
        Task DeleteAsync(int id, string indexName);
        Task DeleteByFieldAsync<TField>(TField value, string indexName, Expression<Func<T, TField>> field);
        Task<long> GetTotalProductCountAsync(string indexName);
    }
}
