using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Debug endpoint for Saritasa messages viewer.
    /// </summary>
    public static class DebugEndpointConfig
    {
        public static Saritasa.Tools.Messages.Endpoints.WebEndpoint WebEndpoint { get; private set; }

        public static void Register()
        {
            var settingValue = ConfigurationManager.AppSettings.Get("EnableDebugEndpoint");
            if (!string.IsNullOrEmpty(settingValue) && settingValue.ToLowerInvariant().Trim() == "true")
            {
                WebEndpoint = new Saritasa.Tools.Messages.Endpoints.WebEndpoint();
                WebEndpoint.RegisterPipelines(
                    DependencyResolver.Current.GetService<Saritasa.Tools.Commands.ICommandPipeline>(),
                    DependencyResolver.Current.GetService<Saritasa.Tools.Events.IEventPipeline>(),
                    DependencyResolver.Current.GetService<Saritasa.Tools.Queries.IQueryPipeline>()
                );
                WebEndpoint.Start();
            }

            settingValue = ConfigurationManager.AppSettings.Get("DebugMessagesDir");
            if (!string.IsNullOrEmpty(settingValue) && Directory.Exists(settingValue))
            {
                var fileMessageRepository = new Saritasa.Tools.Messages.Repositories.FileMessageRepository(settingValue);
                var fileRepositoryMiddleware = new Saritasa.Tools.Messages.PipelineMiddlewares.RepositoryMiddleware(fileMessageRepository);
                DependencyResolver.Current.GetService<Saritasa.Tools.Commands.ICommandPipeline>().AppendMiddlewares(fileRepositoryMiddleware);
                DependencyResolver.Current.GetService<Saritasa.Tools.Events.IEventPipeline>().AppendMiddlewares(fileRepositoryMiddleware);
                DependencyResolver.Current.GetService<Saritasa.Tools.Queries.IQueryPipeline>().AppendMiddlewares(fileRepositoryMiddleware);
            }
        }
    }
}
