using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Autofac;
using Saritasa.Tools.Common.Utils;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;
using ZergRushCo.Todosya.Infrastructure;

namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Debug endpoint for Saritasa messages viewer.
    /// </summary>
    public static class DebugEndpointConfig
    {
        public static Saritasa.Tools.Messages.Common.Endpoints.WebEndpoint WebEndpoint { get; private set; }

        /// <summary>
        /// Register and turn on webpoint. EnableDebugEndpoint application setting should be set to true.
        /// </summary>
        public static void Register(IContainer container)
        {
            var settingValue = ConfigurationManager.AppSettings.Get("EnableDebugEndpoint");
            var pipelinesContainer = container.Resolve<IMessagePipelineContainer>();
            if (StringUtils.ParseOrDefault(settingValue, false))
            {
                WebEndpoint = new Saritasa.Tools.Messages.Common.Endpoints.WebEndpoint(
                    new AutofacServiceProviderFactory(container));

                WebEndpoint.RegisterPipelines(pipelinesContainer);
                WebEndpoint.Start();
            }

            settingValue = ConfigurationManager.AppSettings.Get("DebugMessagesDir");
            if (!string.IsNullOrEmpty(settingValue) && Directory.Exists(settingValue))
            {
                var fileMessageRepository = new Saritasa.Tools.Messages.Common.Repositories.FileMessageRepository(settingValue);
                var fileRepositoryMiddleware = new Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(fileMessageRepository);
                foreach (var pipeline in pipelinesContainer.Pipelines)
                {
                    pipeline.AddMiddlewares(fileRepositoryMiddleware);
                }
            }
        }
    }
}
