using Idata.Entities.Core;

namespace Core.Events.Interfaces
{
    /// <summary>
    /// IEventHandlerBase is a generic interface that defines a set of methods to handle events for entities CRUD.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity for which the events will be handled, entity should be child of EntityBase class.</typeparam>
    public interface IEventHandlerBase<TEntity> where TEntity : EntityBase
    {
        /// <summary>
        /// Gets the event base.
        /// </summary>
        /// <returns>An instance of IEventBase</returns>
        public IEventBase<TEntity> getEventBase();

        /// <summary>
        /// Called when an entity is in the process of being created.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity that is being created.</param>
        public void EntityIsCreating(object? sender, TEntity? e);

        /// <summary>
        /// Called when an entity has been created.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity that was created.</param>
        public void EntityWasCreated(object? sender, TEntity? e);

        /// <summary>
        /// Called when an entity is in the process of being updated.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity that is being updated.</param>
        public void EntityIsUpdating(object? sender, TEntity? e);

        /// <summary>
        /// Called when an entity has been updated.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity that was updated.</param>
        public void EntityWasUpdated(object? sender, TEntity? e);

        /// <summary>
        /// Called when an entity is in the process of being deleted.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity that is being deleted.</param>
        public void EntityIsDeleting(object? sender, TEntity? e);

        /// <summary>
        /// Called when an entity has been deleted.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity that was deleted.</param>
        public void EntityWasDeleted(object? sender, TEntity? e);

        /// <summary>
        /// Called for a custom event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public void CustomEvent(object? sender, TEntity? e);
    }


    /// <summary>
    /// Defines the basic functionality for an event handler.
    /// </summary>
    public interface IEventHandlerBase
    {
        /// <summary>
        /// Gets the event base.
        /// </summary>
        /// <returns>The event base.</returns>
        public IEventBase getEventBase();
    }
}
