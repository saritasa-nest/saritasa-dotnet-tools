Pagination
==========

Simplifies pagination. The are several levels of pagination are supported:

- ``TotalCountList<T>``. Developer constructs the target source manually and should provide total count of records. If not provided it will be evaluated.
- ``OffsetLimitList<T>``. Developer provides base source and subset source will be calculated for him based on offset and limit parameters. Total records count will be evaluated as well if not provided.
- ``PagedList<T>``. Developer provides base source and subset source will be calculated for him based on page and page size parameters. Total records count will be evaluated as well if not provided.

    .. image:: pagination.png

Factories
---------

To simplify pagination classes construction there are several factories available: ``TotalCountListFactory``, ``OffsetLimitListFactory`` and ``PagedListFactory``. There are common methods to use:

.. function:: FromSource<T>(collection)

    Creates specific list from collection. The collection may be evaluated during construction.

.. function:: Empty()

    Creates empty list.

.. function:: Create()

    Creates specific list from parameters. Shorthand to simplify type infer so it does not contain any additional logic.

Metadata
--------

The classes are design to be easily serialized. Here is how you can make JSON for pagination:

    .. code-block:: c#

        var all = PagedListFactory.FromSource(products, 2, 10);
        var all2 = all.Convert(p => new ProductWrapper(p));
        var dto = all2.ToMetadataObject();

        var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
        serializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented, serializerSettings);

    .. code-block:: json

        {
          "metadata": {
            "page": 2,
            "pageSize": 10,
            "totalPages": 2,
            "offset": 10,
            "limit": 10,
            "totalCount": 17
          },
          "items": [
            {
              "name": "Radio"
            },
            {
              "name": "Angular"
            },
            {
              "name": "Trio"
            },
            {
              "name": "Life"
            },
            {
              "name": "Quake"
            },
            {
              "name": "Spinner"
            },
            {
              "name": ".NET"
            }
          ]
        }

Examples
--------

Create offset limit enumerable from queryable source. In this case there will be two queries to data souce: get total items and select with limit and offset.

    .. code-block:: c#

        var query = Context.JiraMappings.AsQueryable();
        int offset = 0, limit = 10;
        var subset = OffsetLimitListFactory.FromSource(query, offset, limit);
        var pagedSubset = PagedListFactory.FromSource(query, page: 2, pageSize: 10);

Make paged enumerable and then convert result to another type.

    .. code-block:: c#

        var repository = new AnotherProductsRepository();
        var products = repository.GetAll();
        var paged = PagedListFactory.FromSource(products, page: 2, pageSize: 10);
        var paged2 = paged.Convert(p => new ProductWrapper(p));
        var dto = paged2.ToMetadataObject();

Select all data as one page.

    .. code-block:: c#

        var repository = new AnotherProductsRepository();
        var products = repository.GetAll();
        var all = PagedListFactory.AsOnePage(products);
