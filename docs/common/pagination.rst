Pagination
==========

Simplifies pagination. The are several levels of pagination are supported:

- ``TotalCountEnumerable<T>``. Developer constructs the target source manually and should provide total count of records. If not provided it will be evaluated.
- ``OffsetLimitEnumerable<T>``. Developer provides base source and subset source will be calculated for him based on offset and limit parameters. Total records count will be evaluated as well if not provided.
- ``PagedEnumerable<T>``. Developer provides base source and subset source will be calculated for him based on page and page size parameters. Total records count will be evaluated as well if not provided.

    .. image:: pagination.png

Examples
--------

Create offset limit enumerable from queryable source. In this case there will be two queries to data souce: get total items and select with limit and offset.

    .. code-block:: c#

        var query = Context.JiraMappings.AsQueryable();
        int offset = 0, limit = 10;
        var subset = OffsetLimitEnumerable.Create(query, offset, limit);
        var subset2 = PagedEnumerable.Create(query, page: 2, pageSize: 10);

Make paged enumerable and then convert result to another type.

    .. code-block:: c#

        var repository = new AnotherProductsRepository();
        var products = repository.GetAll();
        var paged = PagedEnumerable.Create(products, 2, 10);
        var paged2 = paged.CastMetadataEnumerable(p => new ProductWrapper(p));
        var dto = paged2.ToMetadataObject();

Select all data as one page.

    .. code-block:: c#

        var repository = new AnotherProductsRepository();
        var products = repository.GetAll();
        var all = PagedEnumerable.CreateAndReturnAll(products);
