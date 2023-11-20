using Nest;
using RealTimeIndexing.Services.ElasticSearch;

namespace ElasticNetCore.Services
{

    public class ElasticsearchService<T> : IElasticsearchService<T> where T : class, new()
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

            var settings = new ConnectionSettings(new Uri($"{host}:{port}"));

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

        public async Task DeleteIndex(string indexName) 
        {
            await _client.Indices.DeleteAsync(indexName);
        }

        public async Task<T> GetByIdAsync(int id, string indexName)
        {
            var response = await _client.GetAsync<T>(id, s => s.Index(indexName));
            return response.Source;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string indexName)
        {
            var totalCount = await GetTotalProductCountAsync(indexName);
            if (totalCount == 0) return Enumerable.Empty<T>();
            var response = await _client.SearchAsync<T>(s => s.Index(indexName).Size((int?)totalCount).MatchAll());
            return response.Documents;
        }

        public async Task AddManyAsync(IEnumerable<T> entities, string indexName)
        {
            var bulkIndexResponse = await _client.IndexManyAsync(entities, indexName);

            if (!bulkIndexResponse.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {bulkIndexResponse.DebugInformation}");
            }
        }

        public async Task AddAsync(T entity, string indexName)
        {
            var response = await _client.IndexAsync(entity, i => i.Index(indexName));
            if (!response.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {response.DebugInformation}");
            }
        }

        public async Task UpdateAsync(int id, T entity, string indexName)
        {
            var response = await _client.UpdateAsync<T>(id, u => u.Index(indexName).Doc(entity).RetryOnConflict(3));
            if (!response.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {response.DebugInformation}");
            }
        }

        public async Task DeleteAsync(int id, string indexName)
        {
            var response = await _client.DeleteAsync<T>(id, d => d.Index(indexName));
            if (!response.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {response.DebugInformation}");
            }
        }

        public async Task<long> GetTotalProductCountAsync(string indexName)
        {
            var countResponse = await _client.CountAsync<T>(c => c.Index(indexName));

            if (!countResponse.IsValid)
            {
                return 0;
            }

            return countResponse.Count;
        }
    }
}