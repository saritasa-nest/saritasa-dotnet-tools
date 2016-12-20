using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    [JsonConverter(typeof(SearchQuerySerializer))]
    public class SearchQuery
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
                throw new ArgumentException(nameof(from));
            }
            From = from;
            return this;
        }

        public SearchQuery WithSize(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException(nameof(size));
            }

            Size = size;
            return this;
        }

        public IFieldNameQuery[] FieldQueries { get; }
    }
}
