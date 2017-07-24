namespace Saritasa.BoringWarehouse.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    using Tools.Messages.Abstractions;
    using Tools.Domain.Exceptions;

    using Core;
    using Domain.Users.Commands;
    using Domain.Users.Entities;
    using Domain.Users.Queries;
    using Models;

    /// <summary>
    /// User controller.
    /// </summary>
    [AllowAnonymous]
    public class UserController : Controller
    {
        readonly IPipelineService pipelineService;
        readonly UserQueries userQueries;

        public UserController(IPipelineService pipelineService, UserQueries userQueries)
        {
            this.pipelineService = pipelineService;
            this.userQueries = userQueries;
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Register")]
        public ActionResult RegisterPost(CreateUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            try
            {
                pipelineService.HandleCommand(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(command);
            }
            return Redirect("~");
        }

        [System.Web.Mvc.Authorize]
        [ActionName("Profile")]
        public ActionResult UserProfile()
        {
            TicketUserData userData = TicketUserData.FromContext(HttpContext);
            User user = userQueries.GetById(userData.UserId);
            return View(new UserProfileVM(user));
        }

        [System.Web.Mvc.Authorize]
        [HttpPost]
        [ActionName("Profile")]
        public ActionResult UserProfilePost(UserProfileVM userProfile)
        {
            if (!ModelState.IsValid)
            {
                return View(userProfile);
            }
            try
            {
                var userData = TicketUserData.FromContext(HttpContext);
                var command = new UpdateUserCommand
                {
                    Email = userProfile.Email,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Password = userProfile.Password,
                    Phone = userProfile.Phone,
                    UserId = userData.UserId
                };
                pipelineService.HandleCommand(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(userProfile);
            }
            return Redirect("~");
        }

        public ActionResult Login(string returnUrl = "~")
        {
            if (Request.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        public ActionResult LoginPost(LoginUserCommand command, string returnUrl = "~")
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            pipelineService.HandleCommand(command);
            if (!command.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Incorrect login or password");
                return View(command);
            }

            AddCookieAndRedirect(command.User);
            return Redirect(returnUrl);
        }

        [NonAction]
        private void AddCookieAndRedirect(User user)
        {
            var customData = new TicketUserData { UserId = user.Id, UserRole = user.Role };
            var expiration = DateTime.Now.AddDays(1);
            var ticket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, expiration, false, customData.ToString());
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            Response.Cookies.Add(cookie);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
