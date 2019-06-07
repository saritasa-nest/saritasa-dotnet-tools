// Copyright (c) 2017-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Saritasa.Tools.Messages.TestRuns.Loaders
{
    /// <summary>
    /// Retrieve only specified set of files.
    /// </summary>
    public class FilesLoader : ITestRunLoader
    {
        /// <summary>
        /// Files.
        /// </summary>
        public string[] Files { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="files">Set of files.</param>
        public FilesLoader(params string[] files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }
            this.Files = files;
        }

        /// <inheritdoc />
        public IEnumerable<TestRun> Get()
        {
            foreach (string file in Files)
            {
                if (!File.Exists(file))
                {
                    throw new TestRunException($"Cannot find file {file}.");
                }

                using (var fileStream = File.OpenText(file))
                {
                    yield return TestRun.Load(fileStream);
                }
            }
        }
    }
}
