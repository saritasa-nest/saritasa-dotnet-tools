// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message processing status.
    /// </summary>
    public enum ProcessingStatus : byte
    {
        /// <summary>
        /// Default command state.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// The command in a processing state.
        /// </summary>
        Processing,

        /// <summary>
        /// Command has been completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Command has been failed while execution. Mostly exception occured
        /// in handler.
        /// </summary>
        Failed,

        /// <summary>
        /// Command has been rejected. It may be validation error.
        /// </summary>
        Rejected,
    }
}
