using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Test controller. For demo only.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/test")]
    public class ApiTestController : BaseController
    {
        private readonly IAppUnitOfWorkFactory appUnitOfWorkFactory;

        public ApiTestController(
            IMessagePipelineService pipelineService,
            ILoggerFactory loggerFactory,
            IAppUnitOfWorkFactory appUnitOfWorkFactory) : base(pipelineService, loggerFactory)
        {
            this.appUnitOfWorkFactory = appUnitOfWorkFactory;
        }

        /// <summary>
        /// Test repository api.
        /// </summary>
        /// <returns>Nothing.</returns>
        [HttpGet]
        [Route("repository")]
        public async Task<JsonResult> TestRepositoryApi()
        {
            var cts = new CancellationTokenSource();
            using (var uow = appUnitOfWorkFactory.Create())
            {
                var test1 = uow.TaskRepository.Get(1);
                var test2 = uow.TaskRepository.GetAll();
                var test3 = uow.TaskRepository.GetAll(inc => inc.Project, inc => inc.User);
                var test4 = uow.TaskRepository.Find(t => t.IsDone, inc => inc.Project, inc => inc.User);

                var test5 = await uow.TaskRepository.GetAsync(1); // extension
                var test6 = await uow.TaskRepository.GetAsync(cts.Token, 1);
                var test7 = await uow.TaskRepository.GetAllAsync();
                var test8 = await uow.TaskRepository.GetAllAsync(inc => inc.Project, inc => inc.User); // extension
                var test9 = await uow.TaskRepository.GetAllAsync(cts.Token);
                var test10 = await uow.TaskRepository.FindAsync(t => t.IsDone, inc => inc.Project, inc => inc.User); // extension
                var test11 = await uow.TaskRepository.FindAsync(
                    t => t.IsDone,
                    cts.Token,
                    inc => inc.Project, inc => inc.User);
            }

            return null;
        }
    }
}
