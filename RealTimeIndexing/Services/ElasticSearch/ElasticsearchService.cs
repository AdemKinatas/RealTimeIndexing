using Nest;
using RealTimeIndexing.Services.ElasticSearch;
using System.Linq.Expressions;

namespace ElasticNetCore.Services
{

    public class ElasticsearchService<TEntity> : IElasticsearchService<TEntity> where TEntity : class, new()
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

        public async Task<TEntity> GetByIdAsync(int id, string indexName)
        {
            var response = await _client.GetAsync<TEntity>(id, s => s.Index(indexName));
            return response.Source;
        }

        public async Task<TEntity> GetByFieldAsync<TField>(Expression<Func<TEntity, TField>> field, TField value, string indexName)
        {
            var searchResponse = await _client.SearchAsync<TEntity>(s => s
                .Index(indexName)
                .Query(q => q
                    .Match(m => m
                        .Field(field)
                        .Query(value.ToString())
                    )
                )
            );

            if (searchResponse.IsValid && searchResponse.Documents.Any())
            {
                return searchResponse.Documents.First();
            }

            return null; // Belirtilen değer ile eşleşen belge bulunamadı
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(string indexName)
        {
            var totalCount = await GetTotalProductCountAsync(indexName);
            if (totalCount == 0) return Enumerable.Empty<TEntity>();
            var response = await _client.SearchAsync<TEntity>(s => s.Index(indexName).Size((int?)totalCount).MatchAll());
            return response.Documents;
        }

        public async Task AddManyAsync(IEnumerable<TEntity> entities, string indexName)
        {
            var bulkIndexResponse = await _client.IndexManyAsync(entities, indexName);

            if (!bulkIndexResponse.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {bulkIndexResponse.DebugInformation}");
            }
        }

        public async Task AddAsync(TEntity entity, string indexName)
        {
            var response = await _client.IndexAsync(entity, i => i.Index(indexName));
            if (!response.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {response.DebugInformation}");
            }
        }

        public async Task UpdateAsync(int id, TEntity entity, string indexName)
        {
            var response = await _client.UpdateAsync<TEntity>(id, u => u.Index(indexName).Doc(entity).RetryOnConflict(3));
            if (!response.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {response.DebugInformation}");
            }
        }

        public async Task UpdateByFieldAsync<TField>(TField value, TEntity entity, string indexName, Expression<Func<TEntity, TField>> field)
        {
            var searchResponse = await _client.SearchAsync<TEntity>(s => s
                .Index(indexName)
                .Query(q => q
                    .Match(m => m
                        .Field(field)
                        .Query(value.ToString())
                    )
                )
            );

            if (searchResponse.IsValid && searchResponse.Documents.Any())
            {
                var id = searchResponse.Hits.FirstOrDefault().Id;
                var response = await _client.UpdateAsync<TEntity>(id, u => u.Index(indexName).Doc(entity).RetryOnConflict(3));
                if (!response.IsValid)
                {
                    // Handle the error
                    throw new Exception($"Elasticsearch error: {response.DebugInformation}");
                }
            }
            else
            {
                // Belirtilen değer ile eşleşen belge bulunamadı, isteğe bağlı olarak işlem yapılabilir
            }
        }

        public async Task DeleteAsync(int id, string indexName)
        {
            var response = await _client.DeleteAsync<TEntity>(id, d => d.Index(indexName));
            if (!response.IsValid)
            {
                // Handle the error
                throw new Exception($"Elasticsearch error: {response.DebugInformation}");
            }
        }

        public async Task DeleteByFieldAsync<TField>(TField value, string indexName, Expression<Func<TEntity, TField>> field)
        {
            // Belirtilen alan adı ve değere sahip belgeyi bul
            var searchResponse = await _client.SearchAsync<TEntity>(s => s
                .Index(indexName)
                .Query(q => q
                    .Match(m => m
                        .Field(field) // Bu kısmı, Elasticsearch belgenizdeki alan adına göre güncelleyin
                        .Query(value.ToString())
                    )
                )
            );

            if (searchResponse.IsValid && searchResponse.Documents.Any())
            {
                // Belgeyi sil
                var id = searchResponse.Hits.FirstOrDefault().Id;
                var deleteResponse = await _client.DeleteAsync<TEntity>(id, d => d.Index(indexName));

                if (!deleteResponse.IsValid)
                {
                    // Hata durumunda işlemleri yönet
                    throw new Exception($"Elasticsearch error: {deleteResponse.DebugInformation}");
                }
            }
            else
            {
                // Belirtilen alan ve değere sahip belge bulunamadı
            }
        }

        public async Task<long> GetTotalProductCountAsync(string indexName)
        {
            var countResponse = await _client.CountAsync<TEntity>(c => c.Index(indexName));

            if (!countResponse.IsValid)
            {
                return 0;
            }

            return countResponse.Count;
        }
    }
}