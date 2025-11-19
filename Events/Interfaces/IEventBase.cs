namespace Core.Events.Interfaces
{
    /// <summary>
    /// The IEventBase interface is used to define a set of events and methods for firing events related to an entity CRUD.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity for which events are defined, entity should be child of EntityBase class.</typeparam>
    public interface IEventBase<TEntity>
    {
        /// <summary>
        /// The event that is raised when an entity is being created.
        /// </summary>
        public event EventHandler<TEntity?>? EntityIsCreating;
        /// <summary>
        /// The event that is raised when an entity has been created.
        /// </summary>
        public event EventHandler<TEntity?>? EntityWasCreated;

        /// <summary>
        /// The event that is raised when an entity is being updated.
        /// </summary>
        public event EventHandler<TEntity?>? EntityIsUpdating;
        /// <summary>
        /// The event that is raised when an entity has been updated.
        /// </summary>
        public event EventHandler<TEntity?>? EntityWasUpdated;

        /// <summary>
        /// The event that is raised when an entity is being deleted.
        /// </summary>
        public event EventHandler<TEntity?>? EntityIsDeleting;
        /// <summary>
        /// The event that is raised when an entity has been deleted.
        /// </summary>
        public event EventHandler<TEntity?>? EntityWasDeleted;

        /// <summary>
        /// The event that is raised when an entity is being restored.
        /// </summary>
        public event EventHandler<TEntity?>? EntityIsRestoring;

        /// <summary>
        /// The event that is raised when an entity has been restored.
        /// </summary>
        public event EventHandler<TEntity?>? EntityWasRestored;

        /// <summary>
        /// The event that is raised for custom purposes.
        /// </summary>
        public event EventHandler<TEntity?>? CustomEvent;

        /// <summary>
        /// Raises the EntityIsCreating event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityIsCreating(TEntity? common);
        /// <summary>
        /// Raises the EntityWasCreated event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityWasCreated(TEntity? common);
        /// <summary>
        /// Raises the EntityIsUpdating event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityIsUpdating(TEntity? common);
        /// <summary>
        /// Raises the EntityWasUpdated event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityWasUpdated(TEntity? common);
        /// <summary>
        /// Raises the EntityIsDeleting event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityIsDeleting(TEntity? common);
        /// <summary>
        /// Raises the EntityWasDeleted event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityWasDeleted(TEntity? common);

        /// <summary>
        /// Raises the EntityIsRestoring event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityIsRestoring(TEntity? common);
        /// <summary>
        /// Raises the EntityWasRestored event.
        /// </summary>
        /// <param name="common">The entity for which the event is being raised.</param>
        public void FireEntityWasRestored(TEntity? common);
        /// <summary>
        /// This method fires the "CustomEvent" event, meant to handle specific events.
        /// </summary>
        /// <param name="common">The entity for the custom event.</param>
        public void FireCustomEvent(TEntity? common);
    }


    /// <summary>
    /// Interface IEventBase is used to define events and the corresponding methods to fire events.
    /// </summary>
    public interface IEventBase
    {
        /// <summary>
        /// This event is fired when a URL request is being parsed.
        /// </summary>
        public event EventHandler<object?>? URLRequestBeingParsed;

        /// <summary>
        /// This method fires the "URLRequestBeingParsed" event, meant to inform that UrlRequestBase instance is going to be parsed.
        /// </summary>
        /// <param name="common">The object for the URL request being parsed event.</param>
        public void FireURLRequestBeingParsed(object? common);
    }
}
