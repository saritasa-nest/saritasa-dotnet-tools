// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Saritasa.Tools.Messages.TestRuns.Loaders
{
    /// <summary>
    /// Load test data from stream.
    /// </summary>
    public class StreamLoader : ITestRunLoader
    {
        private readonly Stream stream;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        public StreamLoader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("Input stream should be readable.", nameof(stream));
            }
            this.stream = stream;
        }

        /// <inheritdoc />
        public IEnumerable<TestRun> Get()
        {
            using (var streamReader = new StreamReader(this.stream))
            {
                return new TestRun[] { TestRun.Load(streamReader) };
            }
        }
    }
}
