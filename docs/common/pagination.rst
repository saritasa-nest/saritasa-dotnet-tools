Pagination
==========

Simplify pagination.

.. class:: PagedEnumerable

    The class helps to make paged enumerables. It wraps current page and page size. If not specified default page is first and default page size is 100. If ``totalPages`` parameter is below or equal zero it will be automatically populated with ``Count()`` method.

    .. function:: PagedEnumerable(IEnumerable<T> source, int page, int pageSize, int totalPages)

        Creates instance of class. There are two examples of usage:

            .. code-block:: c#

                IEnumerable<string> list = ...
                // Сreates a paged list on page 2 where page size is 20.
                PageEnumerable<string> pagedList = new PagedEnumerable<string>(list, 2, 20);
                // Another way with extension method.
                pagedList = list.AsPage(2, 20);
                Grid.DataSource = pagedList;

    .. function:: PagedEnumerable<T> Create(IEnumerable<T> pagedSource, int page, int pageSize, int totalPages)

        Creates an instance without any queries. It only fills internal properies.

    .. function:: PagedEnumerable<T> CreateAndReturnAll([NotNull] IEnumerable<T> source)

        Returns paged enumerable that contains only one page with all data on it.

    .. function:: PagedMetadata GetMetadata()

        Returns special formatted object that contains metadata information about paged enumerable: page size, current page and total pages.

    .. function:: PagedEnumerable<TTarget> Map<TTarget>(Func<T, TTarget> map)
                  PagedEnumerable<TTarget> Map<TTarget>()

        Converts current paged enumerable to another paged enumerable with another source type. Metadata is copied. Needs if you want to convert the type of page enumerable without metadata change.
