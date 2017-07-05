Pagination
==========

Simplifies pagination. The are several levels of pagination are supported:

- ``TotalCountEnumerable<T>``. Developer constructs the target source manually and should provide total count of records. If not provided it will be evaluated.
- ``OffsetLimitEnumerable<T>``. Developer provides base source and subset source will be calculated for him based on offset and limit parameters. Total records count will be evaluated as well if not provided.
- ``PagedEnumerable<T>``. Developer provides base source and subset source will be calculated for him based on page and page size parameters. Total records count will be evaluated as well if not provided.

Examples
--------

Create offset limit enumerable from queryable source. In this case there will be two queries to data souce: get total items and select with limit and offset.

    .. code-block:: c#

        var query = Context.JiraMappings.AsQueryable();
        int offset = 0, limit = 10;
        OffsetLimitEnumerable<JiraMapping> querySubset = new OffsetLimitEnumerable<JiraMapping>(query, offset, limit);

The same example using extension.

    .. code-block:: c#

        var query = Context.JiraMappings.AsQueryable();
        int offset = 0, limit = 10;
        OffsetLimitEnumerable<JiraMapping> querySubset = query.AsOffsetLimit(offset, limit);

Make paged enumerable and then convert result to another type.

    .. code-block:: c#

        var query = Context.User.AsQueryable();
        PagedEnumerable<User> paged = query.AsPaged(1, 50);
        PagedEnumerable<UserWithDepartment> paged2 = PagedEnumerable<JiraMapping>.Create(paged.Select(u => u.Id), paged);

Extensions
----------

There are also extension methods to simplify pagination enumerable creation.
