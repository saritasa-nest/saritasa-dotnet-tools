// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    [JsonConverter(typeof(RangeQuerySerializer))]
    internal class RangeQuery : IFieldNameQuery
    {
        public string Field { get; set; }

        public object GreaterThanOrEqualTo { get; set; }

        public object LessThanOrEqualTo { get; set; }

        public object GreaterThan { get; set; }

        public object LessThan { get; set; }
    }
}
