using Core.Events.Interfaces;
using Idata.Entities.Core;

namespace Core.Events
{
    /// <summary>
    /// Base class for managing events for a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to manage events for, is has to be child of EntityBase Class.</typeparam>
    public class EventBase<TEntity> : IEventBase<TEntity> where TEntity : EntityBase
    {
        // Event that is fired before creating an entity
        public event EventHandler<TEntity?>? EntityIsCreating;

        // Event that is fired after an entity has been created
        public event EventHandler<TEntity?>? EntityWasCreated;

        // Event that is fired before updating an entity
        public event EventHandler<TEntity?>? EntityIsUpdating;

        // Event that is fired after an entity has been updated
        public event EventHandler<TEntity?>? EntityWasUpdated;

        // Event that is fired before deleting an entity
        public event EventHandler<TEntity?>? EntityIsDeleting;

        // Event that is fired after an entity has been deleted
        public event EventHandler<TEntity?>? EntityWasDeleted;

        // Event that is fired before restoring an entity
        public event EventHandler<TEntity?>? EntityIsRestoring;

        // Event that is fired after an entity has been restored
        public event EventHandler<TEntity?>? EntityWasRestored;

        // Event for custom actions
        public event EventHandler<TEntity?>? CustomEvent;

        // Method to fire EntityIsCreating event
        public void FireEntityIsCreating(TEntity? common) => EntityIsCreating?.Invoke(this, common);

        // Method to fire EntityWasCreated event
        public void FireEntityWasCreated(TEntity? common) => EntityWasCreated?.Invoke(this, common);

        // Method to fire EntityIsUpdating event
        public void FireEntityIsUpdating(TEntity? common) => EntityIsUpdating?.Invoke(this, common);

        // Method to fire EntityWasUpdated event
        public void FireEntityWasUpdated(TEntity? common) => EntityWasUpdated?.Invoke(this, common);

        // Method to fire EntityIsDeleting event
        public void FireEntityIsDeleting(TEntity? common) => EntityIsDeleting?.Invoke(this, common);

        // Method to fire EntityWasDeleted event
        public void FireEntityWasDeleted(TEntity? common) => EntityWasDeleted?.Invoke(this, common);

        // Method to fire EntityIsRestoring event
        public void FireEntityIsRestoring(TEntity? common) => EntityIsDeleting?.Invoke(this, common);

        // Method to fire EntityWasRestored event
        public void FireEntityWasRestored(TEntity? common) => EntityWasDeleted?.Invoke(this, common);
        // Method to fire CustomEvent event
        public void FireCustomEvent(TEntity? common) => CustomEvent?.Invoke(this, common);

    }
    public class EventBase : IEventBase
    {
        // Event that is fired when a URL request is being parsed
        public event EventHandler<object?>? URLRequestBeingParsed;

        // Method to fire URLRequestBeingParsed event
        public void FireURLRequestBeingParsed(object? common) => URLRequestBeingParsed?.Invoke(this, common);

    }
}
