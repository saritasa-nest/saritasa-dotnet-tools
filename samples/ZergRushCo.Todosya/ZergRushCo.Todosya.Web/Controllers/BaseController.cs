using System.Web.Mvc;
using Saritasa.Tools.Messages.Abstractions;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Base controller that contains common properties.
    /// </summary>
    public class BaseController : Controller
    {
        protected ICommandPipeline CommandPipeline { get; }

        protected IQueryPipeline QueryPipeline { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="commandPipeline">Command pipeline.</param>
        /// <param name="queryPipeline">Query pipeline.</param>
        public BaseController(ICommandPipeline commandPipeline, IQueryPipeline queryPipeline)
        {
            CommandPipeline = commandPipeline;
            QueryPipeline = queryPipeline;
        }
    }
}
