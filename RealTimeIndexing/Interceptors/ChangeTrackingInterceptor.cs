using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RealTimeIndexing.Hubs;

namespace RealTimeIndexing.Interceptors
{
    public class ChangeTrackingInterceptor : SaveChangesInterceptor
    {
        private readonly IndexingHub _indexingHub;

        public ChangeTrackingInterceptor()
        {
            _indexingHub = new IndexingHub();
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var entries = eventData.Context.ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                var entity = entry.Entity; // Değişiklik yapılan entity
                var state = entry.State;   // Entity'nin yeni durumu
                var entityType = entry.Entity.GetType(); // Entity'nin Tipi

                _indexingHub.SendChangeNotification(entityType, state, entity);
            }

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var entries = eventData.Context.ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                var entity = entry.Entity; // Değişiklik yapılan entity
                var state = entry.State;   // Entity'nin yeni durumu
                var entityType = entry.Entity.GetType(); // Entity'nin Tipi

                await _indexingHub.SendChangeNotification(entityType, state, entity);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
