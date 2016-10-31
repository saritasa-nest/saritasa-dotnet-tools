using System.Web.Mvc;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Queries;

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
