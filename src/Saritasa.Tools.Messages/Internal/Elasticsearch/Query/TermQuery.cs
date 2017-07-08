// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    [JsonConverter(typeof(TermQuerySerializer))]
    internal class TermQuery : IFieldNameQuery
    {
        /// <summary>
        /// Term query field.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Term query value.
        /// </summary>
        public object Value { get; set; }
    }
}
