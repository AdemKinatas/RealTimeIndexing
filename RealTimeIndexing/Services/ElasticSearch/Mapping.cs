using Nest;
using RealTimeIndexing.Entities;

namespace RealTimeIndexing.Services.ElasticSearch
{
    public static class Mapping
    {
        public static CreateIndexDescriptor ProductMapping(this CreateIndexDescriptor descriptor)
        {
            return descriptor.Map<Product>(m => m.Properties(p => p
            .Keyword(k => k.Name(n => n.ProductId))
            .Number(num => num.Name(n => n.CategoryId))
            .Text(t => t.Name(n => n.ProductName))
            .Text(t => t.Name(n => n.QuantityPerUnit))
            .Number(num => num.Name(n => n.UnitsInStock))
            .Number(num => num.Name(n => n.UnitPrice))
            ));
        }
    }
}
