using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Web.Models;
using ZergRushCo.Todosya.Domain.UserContext.Services;
using ZergRushCo.Todosya.Domain.UserContext.Entities;
using ZergRushCo.Todosya.Domain.UserContext.Commands;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// User account controller.
    /// </summary>
    [Authorize]
    public class AccountController : BaseController
    {
        readonly SignInManager<User, string> signInManager;

        public AccountController(
            ICommandPipeline commandPipeline,
            IQueryPipeline queryPipeline,
            ILoggerFactory loggerFactory,
            SignInManager<User, string> signInManager) : base(commandPipeline, queryPipeline, loggerFactory)
        {
            this.signInManager = signInManager;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginCommand model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : (ActionResult)RedirectToAction("Index", "Home");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterUserCommand command)
        {
            await HandleCommandAsync(command);
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            await signInManager.SignInAsync(command.User, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || code == null)
            {
                return View("Error");
            }
            var result = await signInManager.UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordCommand command)
        {
            if (ModelState.IsValid)
            {
                var user = await signInManager.UserManager.FindByNameAsync(command.Email);
                if (user == null || !(await signInManager.UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return View("ForgotPasswordConfirmation");
                }
            }

            return View(command);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }
            var user = await signInManager.UserManager.FindByNameAsync(command.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await signInManager.UserManager.ResetPasswordAsync(user.Id, command.Code, command.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                signInManager?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
