using System.Web.Mvc;
using Saritasa.Tools.Messages.Abstractions;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Base controller that contains common properties.
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly ICommandPipeline CommandPipeline;
        protected readonly IQueryPipeline QueryPipeline;

        public BaseController(ICommandPipeline commandPipeline, IQueryPipeline queryPipeline)
        {
            CommandPipeline = commandPipeline;
            QueryPipeline = queryPipeline;
        }
    }
}
