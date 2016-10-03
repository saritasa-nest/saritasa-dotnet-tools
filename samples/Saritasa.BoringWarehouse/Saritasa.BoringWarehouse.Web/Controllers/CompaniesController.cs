namespace Saritasa.BoringWarehouse.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using Domain.Products.Commands;
    using Domain.Products.Entities;
    using Domain.Products.Queries;
    using Tools.Commands;
    using Tools.Exceptions;

    /// <summary>
    /// Companies controller
    /// </summary>
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ICommandPipeline commandPipline;
        private readonly CompanyQueries companyQueries;

        public CompaniesController(ICommandPipeline commandPipline, CompanyQueries companyQueries)
        {
            if (commandPipline == null)
            {
                throw new ArgumentNullException(nameof(commandPipline));
            }
            this.commandPipline = commandPipline;
            this.companyQueries = companyQueries;
        }

        public ActionResult Index()
        {
            return View(companyQueries.GetAll());
        }

        public ActionResult Details(int id)
        {
            // TODO
            return View();
        }

        [HttpGet]
        [ActionName("Create")]
        public ActionResult Create()
        {
            return View(new CreateCompanyCommand());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public ActionResult CreatePost(CreateCompanyCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }
            try
            {
                command.CreatedByUserId = Core.TicketUserData.FromContext(HttpContext).UserId;
                commandPipline.Handle(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(command);
            }
            return RedirectToAction("Index");
        }

        // GET: Companies/Edit/5
        public ActionResult Edit(int id)
        {
            // TODO
            return View();
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Company company)
        {
            // TODO
            return View(company);
        }

        // GET: Companies/Delete/5
        public ActionResult Delete(int? id)
        {
            // TODO
            return View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // TODO
            return RedirectToAction("Index");
        }
    }
}
