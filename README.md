# RealTimeIndexing

In this project, I utilized .NET 6 along with the Northwind database to enable real-time updates on Elasticsearch based on changes in the database. 
To capture real-time database changes, I first activated the Service Broker service in the database. Then, I managed instant changes using the SqlTableDependency library.

Bu projede, .net 6 ile Northwind veritabanını kullanarak veritabanı üzerinde anlık değişiklikliklerin Elasticsearch üzerinde güncellenmesini sağladım.
Anlık veritabanı değişikliklerini yakalayabilmek için öncelikle veritabanında Service Broker adlı servisi aktif hale getirdim, daha sonra SqlTableDependency kütüphanesini kullanarak anlık değişiklikleri yönettim.
