using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Abstractions.Queries;
using ZergRushCo.Todosya.Domain.UserContext.Commands;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Manage user account controller.
    /// </summary>
    [Authorize]
    public class ManageController : BaseController
    {
        readonly SignInManager<User, string> signInManager;

        readonly Domain.UserContext.Queries.UsersQueries userQueries;

        public ManageController(
            IPipelineService pipelineService,
            ILoggerFactory loggerFactory,
            SignInManager<User, string> signInManager,
            Domain.UserContext.Queries.UsersQueries userQueries) :
            base(pipelineService, loggerFactory)
        {
            this.signInManager = signInManager;
            this.userQueries = userQueries;
        }

        [HttpGet]
        public ActionResult Index(bool isUpdated = false)
        {
            ViewBag.StatusMessage = isUpdated ? "Profile updated" : string.Empty;

            var userId = User.Identity.GetUserId();
            var model = new UpdateUserCommand(PipelineService.Query(userQueries).With(q => q.GetById(userId)));
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(UpdateUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            command.UserId = User.Identity.GetUserId();
            PipelineService.HandleCommand(command);
            return View(command);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(UpdateUserPasswordCommand command)
        {
            command.UserId = User.Identity.GetUserId();
            await HandleCommandAsync(command);
            return View(command);
        }

        public ActionResult SetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(UpdateUserPasswordCommand command)
        {
            command.UserId = User.Identity.GetUserId();
            await HandleCommandAsync(command);
            return View(command);
        }
    }
}
