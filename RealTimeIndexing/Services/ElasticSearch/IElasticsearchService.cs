using Microsoft.AspNetCore.Mvc;
using RealTimeIndexing.Entities;

namespace RealTimeIndexing.Services.ElasticSearch
{
    public interface IElasticsearchService<T> where T : class, new()
    {
        Task CheckIndex(string indexName);
        Task<T> GetByIdAsync(int id, string indexName);
        Task<IEnumerable<T>> GetAllAsync(string indexName);
        Task AddManyAsync(IEnumerable<T> entities, string indexName);
        Task AddAsync(T entity, string indexName);
        Task UpdateAsync(int id, T entity, string indexName);
        Task DeleteAsync(int id, string indexName);
        Task<long> GetTotalProductCountAsync(string indexName);
    }
}
