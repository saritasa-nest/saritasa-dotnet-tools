// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The interface represents the ability of command repository to
    /// to save its internal state to dictionary. Also type shoule define static Create
    /// method to recreate itself from dict.
    /// </summary>
    public interface ICommandRepositoryPersist
    {
        /// <summary>
        /// Save internal state to dict.
        /// </summary>
        /// <param name="dict">Dictionary is properties.</param>
        void Save(IDictionary<string, object> dict);
    }
}
