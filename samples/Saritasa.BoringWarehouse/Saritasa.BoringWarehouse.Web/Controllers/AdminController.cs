namespace Saritasa.BoringWarehouse.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tools.Commands;
    using Tools.Exceptions;
    using System.Web.Mvc;
    using AuthorizeAttribute = Core.AuthorizeAttribute;
    using Domain;
    using Domain.Users.Commands;
    using Domain.Users.Entities;
    using Domain.Users.Queries;
    using Models;

    [Authorize(UserRole.Admin)]
    public class AdminController : Controller
    {
        private readonly ICommandPipeline commandPipeline;
        private readonly UserQueries userQueries;

        public AdminController(ICommandPipeline commandPipline, UserQueries userQueries)
        {
            if (commandPipline == null)
            {
                throw new ArgumentNullException(nameof(commandPipline));
            }
            if (userQueries == null)
            {
                throw new ArgumentNullException(nameof(userQueries));
            }

            this.commandPipeline = commandPipline;
            this.userQueries = userQueries;
        }

        public ActionResult Index()
        {
            return View();
        }

        [ActionName("FindUsersAjax")]
        public JsonResult FindUsersAjax(int draw, int start, int length, jDataTablesSearch search, List<jDataTablesColumn> columns, List<jDataTablesOrder> order)
        {
            var objectQuery = new UsersObjectQuery
            {
                Limit = length,
                Offset = start,
                SearchPattern = search.value,
            };
            if (order.Count > 0)
            {
                jDataTablesOrder orderColumn = order.FirstOrDefault();
                objectQuery.OrderColumn = columns[orderColumn.column].data;
                objectQuery.SortOrderName = orderColumn.dir;
            }
            PagedResult<User> products = userQueries.Search(objectQuery);
            var result = new DataTablesSearchResultViewModel
            {
                data = products.Items,
                draw = draw,
                recordsFiltered = products.Total,
                recordsTotal = products.Total,
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ActionName("DetailsUser")]
        public ActionResult DetailsUser(int id)
        {
            return View(userQueries.GetById(id));
        }

        [ActionName("CreateUser")]
        public ActionResult CreateUser()
        {
            var command = new CreateUserCommand();
            return View(command);
        }

        [HttpPost]
        [ActionName("CreateUser")]
        public ActionResult CreateUserPost(CreateUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }
            try
            {
                commandPipeline.Handle(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(command);
            }
            return RedirectToAction("Index");
        }

        [ActionName("EditUser")]
        public ActionResult EditUser(int id)
        {
            User user = userQueries.GetById(id);
            var command = new UpdateUserCommand(user);
            return View(command);
        }

        [ActionName("EditUser")]
        [HttpPost]
        public ActionResult EditUserPost(UpdateUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }
            try
            {
                commandPipeline.Handle(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(command);
            }
            return RedirectToAction("Index");
        }
    }
}
