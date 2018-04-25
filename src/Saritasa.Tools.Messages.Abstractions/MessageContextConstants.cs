// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message context constants.
    /// </summary>
    public static class MessageContextConstants
    {
        /// <summary>
        /// Specifies key to be used in items to determine what pipeline
        /// should be used to process message.
        /// </summary>
        public const string TypeKey = ".type";

        /// <summary>
        /// Specifies key to be used in items to get user key/value
        /// dictionary with additional processing data. For key/value use <see cref="string" /> type.
        /// </summary>
        public const string DataKey = ".data";

        /// <summary>
        /// Additional parameter provided for middleware for handling.
        /// </summary>
        public const string ParamKey = ".param";

        /// <summary>
        /// Contains <see cref="System.Runtime.ExceptionServices.ExceptionDispatchInfo " />
        /// of original exception.
        /// </summary>
        public const string ExceptionDispatchInfoKey = ".exception-dispatch";

        /// <summary>
        /// Execution processing duration key.
        /// </summary>
        public const string ExecutionDurationKey = ".execution-duration";

        /// <summary>
        /// Result.
        /// </summary>
        public const string ResultKey = "result";

        /// <summary>
        /// Unknown type.
        /// </summary>
        public const byte MessageTypeUnknown = 0;

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

        /// <summary>
        /// Dictionary that maps message type to string code.
        /// </summary>
        public static readonly IDictionary<byte, string> MessageTypeCodes = new Dictionary<byte, string>
        {
            [MessageTypeUnknown] = "unknown",
            [MessageTypeCommand] = "command",
            [MessageTypeQuery] = "query",
            [MessageTypeEvent] = "event"
        };
    }
}
