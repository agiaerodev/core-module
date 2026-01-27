using Idata.Entities.Core;

namespace Core.SyncHandlers.Interfaces
{
    public interface IEntitySyncHandler<TEntity> where TEntity : EntityBase
    {
        Task HandleAsync(dynamic message);
    }
}
