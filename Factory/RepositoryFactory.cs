using Core.Events.Interfaces;
using Core.Interfaces;
using Core.Services.Interfaces;
using Hangfire;
using Idata.Data;
using Idata.Entities.Core;
using Ihelpers.Caching.Interfaces;
using Ihelpers.Interfaces;
using Ihelpers.Messages.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Factory
{
    public class RepositoryFactory<TEntity> where TEntity : EntityBase
    {
        public readonly IdataContext _dbContext;
        public readonly IServiceProvider _serviceProvider;
        public readonly ICacheBase _cache;
        public readonly IEventHandlerBase<TEntity>? _eventHandler;
        public readonly IMessageProvider _messageProvider;
        public readonly IReportingService _reportingService;
        public RepositoryFactory(IdataContext dbContext, 
            IServiceProvider serviceProvider, 
            ICacheBase cache, 
            IEventHandlerBase<TEntity>? eventHandler,
            IMessageProvider messageProvider,
            IReportingService reportingService) 
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _eventHandler = eventHandler;
            _messageProvider = messageProvider;
            _reportingService = reportingService; 
        }


    }
}
