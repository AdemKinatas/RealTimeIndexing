﻿
using ElasticNetCore.Services;
using RealTimeIndexing.Entities;
using RealTimeIndexing.Services.ElasticSearch;
using RealTimeIndexing.Services.RabbitMQ;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;

namespace RealTimeIndexing.SubscribeTableDependency
{
    public class SubscribeProductTableDependency<T> : ISubscribeTableDependency where T : class, new()
    {
        private SqlTableDependency<T> _tableDependency;
        private string _tableName;
        private readonly IElasticsearchService<Product> _productElasticsearchService;
        private readonly IRabitMQProducer _rabbitMQProducer;

        public SubscribeProductTableDependency(IElasticsearchService<Product> productElasticsearchService, IRabitMQProducer rabbitMQProducer)
        {
            _productElasticsearchService = productElasticsearchService;
            _rabbitMQProducer = rabbitMQProducer;
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
                switch (_tableName)
                {
                    case "Products":
                        var product = e.Entity as Product;

                        try
                        {
                            switch (e.ChangeType)
                            {
                                case ChangeType.Insert:
                                    //await _productElasticsearchService.AddAsync(product, _tableName.ToLower());
                                    await _productElasticsearchService.AddByDocumentIdAsync(product, _tableName.ToLower());
                                    break;
                                case ChangeType.Update:
                                    //await _productElasticsearchService.UpdateByFieldAsync(product.ProductId, product, "products", p => p.ProductId);
                                    await _productElasticsearchService.UpdateAsync(product.ProductId, product, "products");
                                    break;
                                case ChangeType.Delete:
                                    //await _productElasticsearchService.DeleteByFieldAsync(product.ProductId, "products", p => p.ProductId);
                                    await _productElasticsearchService.DeleteAsync(product.ProductId, "products");
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = new
                            {
                                TableName = _tableName.ToLower(),
                                ChangeType = e.ChangeType.ToString(),
                                Product = product
                            };
                            _rabbitMQProducer.SendIndexMessage(message);
                        }

                        break;
                    case "Categories":

                        break;
                    default:
                        break;
                }
            }
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            // Log will add
            Console.WriteLine($"{nameof(Product)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
