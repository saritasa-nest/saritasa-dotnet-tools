// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Saritasa.Tools.Messages.TestRuns.Loaders
{
    /// <summary>
    /// Get all .json files within directory and all subdirectories.
    /// </summary>
    public class DirectoryFilesLoader : ITestRunLoader
    {
        private readonly string directory;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="directory">Directory to search files.</param>
        public DirectoryFilesLoader(string directory)
        {
            if (!string.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }
            this.directory = directory;
        }

        /// <inheritdoc />
        public IEnumerable<TestRun> Get()
        {
            string[] allfiles = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
            foreach (string file in allfiles)
            {
                using (var fileStream = File.OpenText(file))
                {
                    yield return TestRun.Load(fileStream);
                }
            }
        }
    }
}
