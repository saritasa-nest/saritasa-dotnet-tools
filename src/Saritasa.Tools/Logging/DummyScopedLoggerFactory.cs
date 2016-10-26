using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Logging
{
    /// <inheritdoc />
    public class DummyScopedLoggerFactory : ILoggerFactory
    {
        /// <inheritdoc />
        public ILogger GetLogger(string name)
        {
            return new DummyScopedLogger();
        }
    }
}
