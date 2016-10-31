AtomicUtils
===========

Methods to work with plain CLR objects.

.. function:: static void Swap<T>(ref T item1, ref T item2)

    Swaps values of two variables.

    .. code-block:: c#

        // without Saritasa extensions, temp variable needed
        int a = 2, b = 5;
        int tmp = a;
        a = b;
        b = tmp;

        // with Sarotasa extensions
        int a = 2, b = 5;
        AtomicUtils.Swap(ref a, ref b);

.. function:: static void SafeSwap<T>(ref T item1, ref T item2)

    Swaps values of two variables. Thread safe.

.. function:: public static void DoWithCAS<T>(ref T location, Func<T, T> func)

    Thread safe implementation CAS (Compare-And-Swap). Get the location variable from memory, perform an action on it and replace. There are also override implementations for double and int. Here is an example of thread safe multiply method:

        .. code-block:: c#

            public static void InterlockedMultiplyInPlace(ref int x, int y)
            {
                AtomicUtils.DoWithCAS(ref x, t => t * y);
            }
