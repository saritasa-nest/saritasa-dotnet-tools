using System;

namespace Saritasa.Tools.Logging
{
    /// <summary>
    /// Presents scope for logger.
    /// </summary>
    public interface IScope : IDisposable
    {
        /// <summary>
        /// Checking if scope already disposed.
        /// This must be controlled in logger implementation for appending scope name in messages.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Name of scope.
        /// </summary>
        string ScopeName { get; }
    }
}
