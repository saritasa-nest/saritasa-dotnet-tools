// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Command repository interface.
    /// </summary>
    public interface ICommandRepository
    {
        /// <summary>
        /// Add command execution result.
        /// </summary>
        /// <param name="result">Command execution result.</param>
        void Add(CommandExecutionResult result);

        /// <summary>
        /// Get command execution results by dates.
        /// </summary>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>Enumerable of command execution results.</returns>
        IEnumerable<CommandExecutionResult> GetByDates(DateTime startDate, DateTime endDate);
    }
}
