using Elasticsearch.Net;
using Nest;
using RealTimeIndexing.Entities;
using RealTimeIndexing.Services.ElasticSearch;

namespace ElasticNetCore.Services
{

    public class ElasticsearchService : IElasticsearchService
    {

        private readonly IConfiguration _configuration;
        private readonly IElasticClient _client;

        public ElasticsearchService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = CreateInstance();
        }

        private ElasticClient CreateInstance()
        {
            string host = _configuration.GetSection("ElasticsearchServer").Get<ElasticsearchServer>().Host;
            string port = _configuration.GetSection("ElasticsearchServer").Get<ElasticsearchServer>().Port;
            string username = _configuration.GetSection("ElasticsearchServer").Get<ElasticsearchServer>().Username;
            string password = _configuration.GetSection("ElasticsearchServer").Get<ElasticsearchServer>().Password;

            var settings = new ConnectionSettings(new Uri(host + ":" + port));

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                settings.BasicAuthentication(username, password);

            return new ElasticClient(settings);
        }


        public async Task CheckIndex(string indexName)
        {
            var any = await _client.Indices.ExistsAsync(indexName);
            if (any.Exists)
                return;

            var response = await _client.Indices.CreateAsync(indexName,
                ci => ci
                    .Index(indexName)
                    .ProductMapping()
                    .Settings(s => s.NumberOfShards(3).NumberOfReplicas(1))
                    );
            return;
        }

        public async Task InsertDocument(string indexName, Product product)
        {

            var response = await _client.CreateAsync(product, q => q.Index(indexName));
            if (response.ApiCall?.HttpStatusCode == 409)
            {
                await _client.UpdateAsync<Product>(response.Id, a => a.Index(indexName).Doc(product));
            }

        }

        public async Task InsertDocuments(string indexName, List<Product> products)
        {
            await _client.IndexManyAsync(products, index: indexName);
        }

        public async Task DeleteDocument(string indexName, int id)
        {
            var response = await _client.DeleteAsync<Product>(id, d => d.Index(indexName));
            // Handle response if needed
        }

        public async Task UpdateDocument(string indexName, int id, Product updatedProduct)
        {
            var response = await _client.UpdateAsync<Product>(id, u => u
                .Index(indexName)
                .Doc(updatedProduct)
                .RetryOnConflict(3) // You can adjust the retry on conflict value as needed
            );
            // Handle response if needed
        }

        public async Task<Product> GetDocument(string indexName, int id)
        {
            var searchResponse = await _client.SearchAsync<Product>(s => s
                .Index(indexName) // Specify the index name
                .Query(q => q.Term(t => t.Field(f => f.ProductId).Value(id)))
            );

            if (searchResponse.IsValid)
            {
                var product = searchResponse.Documents.FirstOrDefault();
                return product; // No need for the null check, if there's no match, this will be null.
            }

            return null;
        }

        public async Task<List<Product>> GetDocuments(string indexName)
        {
            var countResponse = await _client.CountAsync<Product>(c => c.Index(indexName).Query(q => q.MatchAll()));
            var response = await _client.SearchAsync<Product>(q => q.Index(indexName).Size((int?)countResponse.Count).SearchType(SearchType.QueryThenFetch));
            return response.Documents.ToList();
        }
    }
}