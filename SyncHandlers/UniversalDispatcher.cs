using Core.SyncHandlers.Interfaces;
using Ihelpers.Messages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SyncHandlers.Messages
{

    public class UniversalDispatcher : IMessageHandler<JObject>
    {
        private readonly IServiceProvider _serviceProvider;
        public UniversalDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task HandleAsync(JObject message)
        {
            string entityStr = message["entity_target"]?.ToString() ?? message["target_entity"]?.ToString();
            if (string.IsNullOrEmpty(entityStr)) return;

            Type entityType = Type.GetType(entityStr);
            if (entityType == null) return;

            
            Type handlerType = typeof(IEntitySyncHandler<>).MakeGenericType(entityType);

            using (var scope = _serviceProvider.CreateScope())
            {
                var handler = scope.ServiceProvider.GetService(handlerType);
                if (handler != null)
                {
                    var method = handlerType.GetMethod("HandleAsync");
                    await (Task)method.Invoke(handler, new object[] { message });
                }
            }
        }
    }
}
