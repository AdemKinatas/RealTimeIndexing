
using RealTimeIndexing.Entities;
using RealTimeIndexing.Services.ElasticSearch;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;

namespace RealTimeIndexing.SubscribeTableDependency
{
    public class SubscribeProductTableDependency<T> : ISubscribeTableDependency where T : class, new()
    {
        private SqlTableDependency<T> _tableDependency;
        private string _tableName;
        private readonly IElasticsearchService<Product> _productElasticsearchService;

        public SubscribeProductTableDependency(IElasticsearchService<Product> productElasticsearchService)
        {
            _productElasticsearchService = productElasticsearchService;
        }

        public void SubscribeTableDependency(string connectionString, string tableName)
        {
            _tableName = tableName;
            _tableDependency = new SqlTableDependency<T>(connectionString, tableName);
            _tableDependency.OnChanged += TableDependency_OnChanged;
            _tableDependency.OnError += TableDependency_OnError;
            _tableDependency.Start();
        }

        private async void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<T> e)
        {
            if (e.ChangeType != ChangeType.None)
            {
                try
                {
                    switch (_tableName)
                    {
                        case "Products":
                            var product = e.Entity as Product;

                            switch (e.ChangeType)
                            {
                                case ChangeType.Insert:
                                    await _productElasticsearchService.AddAsync(product, _tableName.ToLower());
                                    break;
                                case ChangeType.Update:
                                    await _productElasticsearchService.UpdateAsync(product.ProductId, product, _tableName.ToLower());
                                    break;
                                case ChangeType.Delete:
                                    await _productElasticsearchService.DeleteAsync(product.ProductId, _tableName.ToLower());
                                    break;
                                default:
                                    break;
                            }

                            break;
                        case "Categories":

                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    // Kuyruğa eklenecek
                }
            }
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(Product)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
