// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    using Newtonsoft.Json;

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
