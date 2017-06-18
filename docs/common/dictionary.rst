Dictionary
==========

.. function:: TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> target, TKey key, TValue defaultValue)

    Tries to get the value by key. If key is not presented to dictionary returns ``defaultValue``.
