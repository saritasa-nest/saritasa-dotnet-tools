// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    [JsonObject]
    internal class Rsid
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("date_from")]
        public long From { get; set; }

        [JsonProperty("date_to")]
        public long To { get; set; }

        [JsonProperty("elapsed_time")]
        public double ElapsedTime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return string.Format("rsid={0}", Id);
        }
    }
}
