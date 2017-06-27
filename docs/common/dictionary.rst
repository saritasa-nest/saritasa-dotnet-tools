Dictionary
==========

.. function:: TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> target, TKey key, TValue defaultValue)

    Tries to get the value by key. If key is not presented to dictionary returns ``defaultValue``.

.. function:: TValue AddOrUpdate<TKey, TValue>(IDictionary<TKey, TValue> target, TKey key, Func<TKey, TValue, TValue> updateFunc, TValue defaultValue)

    Adds a key/value pair to the ``IDictionary{TKey,TValue}`` if the key does not already exist, or updates a key/value pair in the ``IDictionary{TKey,TValue}`` if the key already exists. The default value will be used if key is missed to the ``IDictionary{TKey,TValue}``.
