ObjectUtils
===========

.. function:: Func<T> CreateTypeFactory<T>() where T : new()

    Generate factory method. For example new() generic constraint uses ``System.Activator``
    class for creating new instances. It can be optimized by making factory delegate.
