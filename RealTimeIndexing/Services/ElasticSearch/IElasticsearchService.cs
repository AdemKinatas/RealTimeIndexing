using RealTimeIndexing.Entities;

namespace RealTimeIndexing.Services.ElasticSearch
{
    public interface IElasticsearchService
    {
        Task CheckIndex(string indexName);
        Task InsertDocument(string indexName, Product product);
        Task InsertDocuments(string indexName, List<Product> products);
        Task<Product> GetDocument(string indexName, int id);
        Task<List<Product>> GetDocuments(string indexName);
    }
}
