// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Mvc.Utils
{
    using System;
    using System.Web.Mvc;

    /// <summary>
    /// Overrides default behavior to get view files from ~/View folder.
    /// </summary>
    public class DifferentFolderRazorViewEngine : RazorViewEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentFolderRazorViewEngine" /> class.
        /// </summary>
        /// <param name="path">The path. Must be in format "~/MyPath".</param>
        public DifferentFolderRazorViewEngine(string path) : base()
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            path = path.Trim();

            ViewLocationFormats = new[]
            {
                path + "/{1}/{0}.cshtml",
                path + "/{1}/{0}.vbhtml",
                path + "/Shared/{0}.cshtml",
                path + "/Shared/{0}.vbhtml"
            };
            MasterLocationFormats = new[]
            {
                path + "/{1}/{0}.cshtml",
                path + "/{1}/{0}.vbhtml",
                path + "/Shared/{0}.cshtml",
                path + "/Shared/{0}.vbhtml"
            };
            PartialViewLocationFormats = new[]
            {
                path + "/{1}/{0}.cshtml",
                path + "/{1}/{0}.vbhtml",
                path + "/Shared/{0}.cshtml",
                path + "/Shared/{0}.vbhtml"
            };
        }
    }
}
