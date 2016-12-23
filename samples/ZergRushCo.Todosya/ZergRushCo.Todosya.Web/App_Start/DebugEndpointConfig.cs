using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Saritasa.Tools.Messages.Abstractions;

namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Debug endpoint for Saritasa messages viewer.
    /// </summary>
    public static class DebugEndpointConfig
    {
        public static Saritasa.Tools.Messages.Common.Endpoints.WebEndpoint WebEndpoint { get; private set; }

        /// <summary>
        /// Register and turn on webpoint. EnableDebugEndpoint application setting shoul be set to true.
        /// </summary>
        public static void Register()
        {
            var settingValue = ConfigurationManager.AppSettings.Get("EnableDebugEndpoint");
            if (!string.IsNullOrEmpty(settingValue) && settingValue.ToLowerInvariant().Trim() == "true")
            {
                WebEndpoint = new Saritasa.Tools.Messages.Common.Endpoints.WebEndpoint();
                WebEndpoint.RegisterPipelines(
                    DependencyResolver.Current.GetService<ICommandPipeline>(),
                    DependencyResolver.Current.GetService<IEventPipeline>(),
                    DependencyResolver.Current.GetService<IQueryPipeline>()
                );
                WebEndpoint.Start();
            }

            settingValue = ConfigurationManager.AppSettings.Get("DebugMessagesDir");
            if (!string.IsNullOrEmpty(settingValue) && Directory.Exists(settingValue))
            {
                var fileMessageRepository = new Saritasa.Tools.Messages.Common.Repositories.FileMessageRepository(settingValue);
                var fileRepositoryMiddleware = new Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(fileMessageRepository);
                DependencyResolver.Current.GetService<ICommandPipeline>().AppendMiddlewares(fileRepositoryMiddleware);
                DependencyResolver.Current.GetService<IEventPipeline>().AppendMiddlewares(fileRepositoryMiddleware);
                DependencyResolver.Current.GetService<IQueryPipeline>().AppendMiddlewares(fileRepositoryMiddleware);
            }
        }
    }
}
