// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    [JsonConverter(typeof(SearchQuerySerializer))]
    internal class SearchQuery
    {
        public SearchQuery(IEnumerable<IFieldNameQuery> fieldQueries)
        {
            FieldQueries = fieldQueries.ToArray();
        }

        public int From { get; private set; }

        public int Size { get; private set; }

        public SearchQuery WithFrom(int from)
        {
            if (from < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(from));
            }
            From = from;
            return this;
        }

        public SearchQuery WithSize(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Size = size;
            return this;
        }

        public IFieldNameQuery[] FieldQueries { get; }
    }
}
