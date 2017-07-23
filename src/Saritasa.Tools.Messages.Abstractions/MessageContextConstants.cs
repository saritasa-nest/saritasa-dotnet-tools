// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message context constants.
    /// </summary>
    public static class MessageContextConstants
    {
        /// <summary>
        /// Execution processing duration key.
        /// </summary>
        public const string ExecutionDurationKey = ".execution-duration";

        /// <summary>
        /// Result.
        /// </summary>
        public const string ResultKey = "result";

        /// <summary>
        /// Command type.
        /// </summary>
        public const byte MessageTypeCommand = 1;

        /// <summary>
        /// Query type.
        /// </summary>
        public const byte MessageTypeQuery = 2;

        /// <summary>
        /// Event type.
        /// </summary>
        public const byte MessageTypeEvent = 3;
    }
}
