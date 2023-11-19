using RealTimeIndexing.SubscribeTableDependency;

namespace RealTimeIndexing.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static void UseSqlTableDependency<T>(this IApplicationBuilder applicationBuilder, string connectionString, string tableName) where T : class, ISubscribeTableDependency
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            var service = (T)serviceProvider.GetService(typeof(T));

            if (service != null)
            {
                service.SubscribeTableDependency(connectionString, tableName);
            }
            else
            {
                Console.WriteLine("Error: Could not resolve the service of type T.");
            }
        }
    }
}
