using Core.Entities;
using Core.Events.Interfaces;
using Idata.Entities.Core;
using System.Reflection.Metadata.Ecma335;

namespace Core.Events
{
    /// <summary>
    /// Base class for handling events for a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to handle events for, must be child of EntityBase class.</typeparam>
    public class EventHandlerBase<TEntity> : IEventHandlerBase<TEntity> where TEntity : EntityBase
    {
        IEventBase<TEntity> _eventBase;

        /// <summary>
        /// Constructor that initializes the event base and sets event handlers.
        /// </summary>
        /// <param name="eventBase">The event base to handle events for.</param>
        public EventHandlerBase(IEventBase<TEntity> eventBase)
        {
            // Subscribe to events
            eventBase.EntityIsCreating += EntityIsCreating;
            eventBase.EntityWasCreated += EntityWasCreated;
            eventBase.EntityIsUpdating += EntityIsUpdating;
            eventBase.EntityWasUpdated += EntityWasUpdated;
            eventBase.EntityIsDeleting += EntityIsDeleting;
            eventBase.EntityWasDeleted += EntityWasDeleted;
            eventBase.EntityIsRestoring += EntityIsRestoring;
            eventBase.EntityWasRestored += EntityWasRestored;
            // Set the event base
            _eventBase = eventBase;
        }

        /// <summary>
        /// Custom event handler.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public void CustomEvent(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity is being created.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityIsCreating(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity is being deleted.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityIsDeleting(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity is being updated.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityIsUpdating(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity is being restored.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityIsRestoring(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }
        /// <summary>
        /// Event handler for when the entity was created.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityWasCreated(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity was deleted.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityWasDeleted(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity was restored.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityWasRestored(object? sender, TEntity? e)
        {
            // Implementation left empty
            return;
        }

        /// <summary>
        /// Event handler for when the entity was updated.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The entity associated with the event.</param>
        public virtual void EntityWasUpdated(object? sender, TEntity? e) 
        {
            // Implementation left empty
            return;
        }
        /// <summary>
        /// returns the internal EventHandler
        /// </summary>
        /// <returns></returns>
        public IEventBase<TEntity> getEventBase()
        {
            return _eventBase;
        }


       
    }

    /// <summary>
    /// EventHandlerBase implements the IEventHandlerBase interface and contains the event handling logic for the `URLRequestBeingParsed` event.
    /// </summary>
    public class EventHandlerBase : IEventHandlerBase
    {
        IEventBase _eventBase;

        /// <summary>
        /// The constructor for the EventHandlerBase class which takes in an IEventBase object.
        /// It initializes the _eventBase instance variable and adds the EventBase_URLRequestBeingParsed method to the `URLRequestBeingParsed` event.
        /// </summary>
        /// <param name="eventBase">An instance of IEventBase used to access the events that need to be handled.</param>
        public EventHandlerBase(IEventBase eventBase)
        {
            eventBase.URLRequestBeingParsed += EventBase_URLRequestBeingParsed;

            _eventBase = eventBase;

        }

        /// <summary>
        /// A virtual method that is called when the `URLRequestBeingParsed` event is triggered.
        /// This method should be overridden in order to provide custom event handling logic.
        /// </summary>
        /// <param name="sender">The source of the event. This is typically the object that raised the event.</param>
        /// <param name="e">The event arguments that contain additional information about the event.</param>
        public virtual void EventBase_URLRequestBeingParsed(object? sender, object? e)
        {
            return;
        }

        /// <summary>
        /// Gets the instance of IEventBase
        /// </summary>
        /// <returns>The instance of IEventBase</returns>
        public IEventBase getEventBase()
        {
            // Returns the instance of IEventBase
            return _eventBase;
        }
    }
}
