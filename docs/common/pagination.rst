Pagination
==========

Simplify pagination.

.. class:: PagedEnumerable

    The class helps to make paged enumerables. It wraps current page and page size. If not specified default page is first and default page size is 100. If ``totalPages`` parameter is below or equal zero it will be automatically populated with ``Count()`` method.

    .. function:: PagedEnumerable(IEnumerable<T> source, int page, int pageSize, int totalPages)

        Creates instance of class. There are two examples of usage:

            .. code-block:: c#

                IEnumerable<string> list = ...
                // creates a paged list on page 2 where page size is 20
                PageEnumerable<string> pagedList = new PagedEnumerable<string>(list, 2, 20);
                // another way with extension method
                pagedList = list.GetPaged(2, 20);
                Grid.DataSource = pagedList;

    .. function:: PagedEnumerable<T> Create(IEnumerable<T> pagedSource, int page, int pageSize, int totalPages)

        Creates an instance without any queries. It only fills internal properies.
