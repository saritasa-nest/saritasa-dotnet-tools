using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Logging.Inetrnal
{
    internal class Scope : IScope
    {
        private bool isDisposed;

        public Scope(string scopeName = null)
        {
            this.ScopeName = scopeName;
        }

        /// <inheritdoc />
        public bool IsDisposed => isDisposed;

        /// <inheritdoc />
        public string ScopeName { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
            }
        }
    }
}
