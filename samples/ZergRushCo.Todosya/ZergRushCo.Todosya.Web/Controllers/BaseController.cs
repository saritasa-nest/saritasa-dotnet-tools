using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Base controller that contains common properties.
    /// </summary>
    public class BaseController : Controller
    {
        readonly ILogger logger;

        protected IMessagePipelineService PipelineService { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public BaseController(
            IMessagePipelineService pipelineService,
            ILoggerFactory loggerFactory)
        {
            PipelineService = pipelineService;
            logger = loggerFactory.CreateLogger<BaseController>();
        }

        /// <summary>
        /// Handle command execution. Fills ModelState in case of DomainException and with
        /// generic message if generic Exception.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="rethrowNotDomainExceptions">Rethrow not domain exceptions. True by default.</param>
        /// <returns>True if there were no exceptions during execution.</returns>
        protected bool HandleCommand(object command, bool rethrowNotDomainExceptions = true)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            var result = true;
            try
            {
                PipelineService.HandleCommand(command);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                result = false;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Server error. Please try again later.");
                if (rethrowNotDomainExceptions)
                {
                    throw;
                }
                logger.LogError(ex.ToString());
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Handle command execution asynchronously. Fills ModelState in case of DomainException and with
        /// generic message if generic Exception.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="rethrowNotDomainExceptions">Rethrow not domain exceptions. True by default.</param>
        /// <returns>True if there were no exceptions during execution.</returns>
        protected async Task<bool> HandleCommandAsync(object command, bool rethrowNotDomainExceptions = true)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            var result = true;
            try
            {
                await PipelineService.HandleCommandAsync(command, CancellationToken.None);
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                result = false;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Server error. Please try again later.");
                if (rethrowNotDomainExceptions)
                {
                    throw;
                }
                logger.LogError(ex.ToString());
                result = false;
            }
            return result;
        }
    }
}
