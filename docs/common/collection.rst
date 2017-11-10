Collection
==========

Set of extensions related to collections (`IEnumerable`, `IList`, etc).

.. function:: IOrderedEnumerable<TSource> Order<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, SortOrder sortOrder)

    Sorts the elements of a sequence in ascending or descending order. ``Ask`` and ``Desc`` are ``SortOrder`` enum members. Also available as extension method.

.. function:: IEnumerable<IQueryable<T>> ChunkSelectRange<T>(IQueryable<T> source, int chunkSize)

    Breaks a list of items into chunks of a specific size and yeilds ``T`` items. Be aware that this method generates one additional query to get total number of collection elements. Default ``chunkSize`` is 1000.

.. function:: IEnumerable<T> ChunkSelect<T>(IQueryable<T> source, int chunkSize)

    Breaks a list of items into chunks of a specific size and yeilds ``T`` items. Default ``chunkSize`` is 1000. Also available as extension method.

.. function:: DistinctBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)

    Returns distinct elements from a sequence by using a specified comparer. Also available as extension method.
