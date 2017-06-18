Collection
==========

Set of extensions related to collections (`IEnumerable`, `IList`, etc).

.. function:: IOrderedEnumerable<TSource> Order<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, SortOrder sortOrder)

    Sorts the elements of a sequence in ascending or descending order. ``Ask`` and ``Desc`` are ``SortOrder`` enum members.

.. function:: IEnumerable<IQueryable<T>> ChunkSelectRange<T>(IQueryable<T> source, int chunkSize)

    Breaks a list of items into chunks of a specific size and yeilds ``T`` items. Default ``chunkSize`` is 1000.

.. function:: IEnumerable<T> ChunkSelect<T>(IQueryable<T> source, int chunkSize)

    Breaks a list of items into chunks of a specific size and yeilds ``T`` items. Default ``chunkSize`` is 1000.
