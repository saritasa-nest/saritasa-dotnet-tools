using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Messages.Queries;
using Saritasa.Tools.Messages.Commands;
using ZergRushCo.Todosya.Web.Models;
using ZergRushCo.Todosya.Domain.Users.Services;
using ZergRushCo.Todosya.Web.Core.Identity;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Manage user account controller.
    /// </summary>
    [Authorize]
    public class ManageController : BaseController
    {
        readonly AppSignInManager signInManager;
        readonly AppUserManager userManager;

        readonly Domain.Users.Queries.UsersQueries userQueries;

        public ManageController(ICommandPipeline commandPipeline, IQueryPipeline queryPipeline) :
            base(commandPipeline, queryPipeline)
        {
        }

        public ManageController(AppUserManager userManager,
            AppSignInManager signInManager,
            Domain.Users.Queries.UsersQueries userQueries,
            IQueryPipeline queryPipeline,
            ICommandPipeline commandPipeline) : base(commandPipeline, queryPipeline)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userQueries = userQueries;
        }

        [HttpGet]
        public ActionResult Index(bool isUpdated = false)
        {
            ViewBag.StatusMessage = isUpdated ? "Profile updated" : string.Empty;

            var userId = Convert.ToInt32(User.Identity.GetUserId());
            var model = new Domain.Users.Commands.UpdateUserCommand(QueryPipeline.Query(userQueries).With(q => q.GetById(userId)));
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Domain.Users.Commands.UpdateUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            command.UserId = Convert.ToInt32(User.Identity.GetUserId());
            CommandPipeline.Handle(command);
            return View(command);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = Convert.ToInt32(User.Identity.GetUserId());
            var result = await userManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = "PasswordHash has been changed" });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        public ActionResult SetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = Convert.ToInt32(User.Identity.GetUserId());
                var result = await userManager.AddPasswordAsync(userId, model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = "PasswordHash has been successfully set" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            // if we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
