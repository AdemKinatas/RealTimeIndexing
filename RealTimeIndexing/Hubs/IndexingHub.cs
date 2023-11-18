using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Entities;

namespace RealTimeIndexing.Hubs
{
    public class IndexingHub : Hub
    {
        public async Task SendChangeNotification(Type type, EntityState state, Object entity)
        {
            switch (type.Name)
            {
                case "Product":

                    Product product = (Product)entity;

                    switch (state)
                    {
                        case EntityState.Added:

                            try
                            {

                            }
                            catch (Exception ex)
                            {

                            }

                            break;
                        case EntityState.Modified:

                            try
                            {

                            }
                            catch (Exception ex)
                            {

                            }

                            break;
                        case EntityState.Deleted:

                            try
                            {

                            }
                            catch (Exception ex)
                            {

                            }

                            break;
                        default:
                            break;
                    }

                    break;


                default:
                    break;
            }
        }
    }
}
