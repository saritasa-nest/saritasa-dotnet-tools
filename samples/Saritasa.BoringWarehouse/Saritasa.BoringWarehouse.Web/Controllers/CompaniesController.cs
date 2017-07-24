namespace Saritasa.BoringWarehouse.Web.Controllers
{
    using System;
    using System.Web.Mvc;

    using Tools.Messages.Abstractions;
    using Tools.Domain.Exceptions;

    using Domain.Products.Commands;
    using Domain.Products.Entities;
    using Domain.Products.Queries;

    /// <summary>
    /// Companies controller
    /// </summary>
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly IPipelineService pipelineService;
        private readonly CompanyQueries companyQueries;

        public CompaniesController(IPipelineService pipelineService, CompanyQueries companyQueries)
        {
            if (pipelineService == null)
            {
                throw new ArgumentNullException(nameof(pipelineService));
            }
            this.pipelineService = pipelineService;
            this.companyQueries = companyQueries;
        }

        // GET: Companies/Index
        public ActionResult Index()
        {
            return View(companyQueries.GetAll());
        }

        // GET: Companies/Details/5
        public ActionResult Details(int id)
        {
            Company company = companyQueries.Get(id);
            return View(company);
        }

        // GET: Companies/Create
        [HttpGet]
        [ActionName("Create")]
        public ActionResult Create()
        {
            return View(new CreateCompanyCommand());
        }

        // POST: Companies/Create
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
                pipelineService.HandleCommand(command);
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
            Company company = companyQueries.Get(id);
            return View(new UpdateCompanyCommand(company));
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UpdateCompanyCommand command)
        {
            try
            {
                pipelineService.HandleCommand(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(command);
            }
            return RedirectToAction("Index");
        }

        // GET: Companies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Company company = companyQueries.Get(id.Value);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Company company = companyQueries.Get(id);

            try
            {
                pipelineService.HandleCommand(new DeleteCompanyCommand(company));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return RedirectToAction("Delete", new { id = id });
            }

            return RedirectToAction("Index");
        }
    }
}
