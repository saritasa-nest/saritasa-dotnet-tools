using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Pipelines service.
    /// </summary>
    public interface IPipelinesService
    {
        /// <summary>
        /// Collection of pipelines.
        /// </summary>
        IMessagePipeline[] Pipelines { get; set; }

        /// <summary>
        /// Current used service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Local key/value collection of objects that are shared across current message scope.
        /// Expect that dictionary implementation can not be thread safe.
        /// </summary>
        IDictionary<object, object> Items { get; set; }
    }
}
