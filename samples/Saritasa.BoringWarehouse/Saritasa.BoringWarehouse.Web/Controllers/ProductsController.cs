using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.BoringWarehouse.Domain;
using Saritasa.BoringWarehouse.Domain.Products.Commands;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Queries;
using Saritasa.BoringWarehouse.Domain.Users.Entities;
using Saritasa.BoringWarehouse.Web.Models;
using AuthorizeAttribute = Saritasa.BoringWarehouse.Web.Core.AuthorizeAttribute;

namespace Saritasa.BoringWarehouse.Web.Controllers
{
    /// <summary>
    /// Products controller
    /// </summary>
    [Authorize(UserRole.Admin, UserRole.Manager)]
    public class ProductsController : Controller
    {
        private readonly ProductQueries productQueries;
        private readonly IMessagePipelineService pipelineService;
        private readonly CompanyQueries companyQueries;

        public ProductsController(IMessagePipelineService pipelineService, ProductQueries productQueries, CompanyQueries companyQueries)
        {
            if (pipelineService == null)
            {
                throw new ArgumentNullException(nameof(pipelineService));
            }
            if (productQueries == null)
            {
                throw new ArgumentNullException(nameof(productQueries));
            }

            this.productQueries = productQueries;
            this.pipelineService = pipelineService;
            this.companyQueries = companyQueries;
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [ActionName("FindProductsAjax")]
        public JsonResult FindProductsAjax(int draw, int start, int length, jDataTablesSearch search, List<jDataTablesColumn> columns, List<jDataTablesOrder> order)
        {
            var objectQuery = new ProductsObjectQuery
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
            PagedResult<Product> products = productQueries.Search(objectQuery);
            var result = new DataTablesSearchResultViewModel
            {
                data = products.Items,
                draw = draw,
                recordsFiltered = products.Total,
                recordsTotal = products.Total,
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("Create")]
        public ActionResult Create()
        {
            var command = new CreateProductCommand();
            command.Companies = companyQueries.GetAll();
            return View(command);
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(CreateProductCommand command)
        {
            if (!ModelState.IsValid)
            {
                // Get companies list again
                command.Companies = companyQueries.GetAll();
                return View(command);
            }
            command.CreatedByUserId = Core.TicketUserData.FromContext(HttpContext).UserId;
            pipelineService.HandleCommand(command);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Edit")]
        public ActionResult Edit(int id)
        {
            Product product = productQueries.Get(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var command = new UpdateProductCommand(product);
            command.Companies = companyQueries.GetAll();
            return View(command);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(UpdateProductCommand command)
        {
            if (!ModelState.IsValid)
            {
                // Get companies list again.
                command.Companies = companyQueries.GetAll();
                return View(command);
            }
            command.UpdatedByUserId = Core.TicketUserData.FromContext(HttpContext).UserId;
            pipelineService.HandleCommand(command);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public ActionResult Details(int id)
        {
            return View(productQueries.Get(id));
        }

        [HttpGet]
        [ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            return View(productQueries.Get(id));
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int id)
        {
            try
            {
                pipelineService.HandleCommand(new DeleteProductCommand(id));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex);
                return View(id);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("DeleteAjax")]
        public JsonResult DeleteAjax(int id)
        {
            try
            {
                pipelineService.HandleCommand(new DeleteProductCommand(id));
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainException ex)
            {
                return Json(new { error = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [ActionName("AddPropertyTemplate")]
        public ActionResult AddPropertyTemplate(int index)
        {
            return PartialView("EditorTemplates/AddPropertyTemplate", new AddProductPropertyTemplate(index));
        }
    }
}
