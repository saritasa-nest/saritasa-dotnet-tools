// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message processing status.
    /// </summary>
    public enum ProcessingStatus : byte
    {
        /// <summary>
        /// Initial state.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// The message is being processing.
        /// </summary>
        Processing,

        /// <summary>
        /// Message processing has been completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Message processing has been failed during execution. There is exception occurred
        /// in handler.
        /// </summary>
        Failed,

        /// <summary>
        /// Message has been rejected. For example it may be validation error.
        /// </summary>
        Rejected
    }
}
