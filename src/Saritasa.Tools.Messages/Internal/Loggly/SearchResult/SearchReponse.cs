// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    internal class SearchReponse
    {
        [JsonProperty("total_events")]
        public int Totals { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("rsid")]
        public Rsid Rsid { get; set; }

        [JsonProperty("events")]
        public List<EventMessage> Events { get; set; }
    }
}
