ObjectUtils
===========

.. function:: Func<T> CreateTypeFactory<T>() where T : new()

    Generate factory method. For example new() generic constraint uses ``System.Activator``
    class for creating new instances. It can be optimized by making factory delegate. Please refer this article for more information: https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/
