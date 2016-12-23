using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Common;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.Tasks.Queries;
using ZergRushCo.Todosya.Domain.Tasks.Commands;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Controller contains actions for projects handling.
    /// </summary>
    public class ProjectsController : BaseController
    {
        public ProjectsController(ICommandPipeline commandPipeline, IQueryPipeline queryPipeline) :
            base(commandPipeline, queryPipeline)
        {
        }

        public ActionResult Index(int page = 1)
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            return View(QueryPipeline.Query<ProjectsQueries>().With(q => q.GetByUser(userId, page, 10)));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new CreateProjectCommand());
        }

        [HttpPost]
        public ActionResult Create(CreateProjectCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            command.CreatedByUserId = Convert.ToInt32(User.Identity.GetUserId());
            CommandPipeline.Handle(command);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var project = QueryPipeline.Query<ProjectsQueries>().With(q => q.GetById(id));
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(new UpdateProjectCommand()
            {
                ProjectId = project.Id,
                Color = project.Color,
                Name = project.Name,
            });
        }

        [HttpPost]
        public ActionResult Edit(int id, UpdateProjectCommand command)
        {
            command.ProjectId = id;
            command.UpdatedByUserId = Convert.ToInt32(User.Identity.GetUserId());
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            CommandPipeline.Handle(command);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Remove(int id)
        {
            var project = QueryPipeline.Query<ProjectsQueries>().With(q => q.GetById(id));
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ActionName("Remove")]
        public ActionResult RemovePost(int id)
        {
            CommandPipeline.Handle(new RemoveProjectCommand()
            {
                ProjectId = id,
                UpdatedByUserId = Convert.ToInt32(User.Identity.GetUserId()),
            });
            return RedirectToAction("Index");
        }
    }
}
