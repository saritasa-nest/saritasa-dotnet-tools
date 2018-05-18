Dictionary
==========

.. function:: TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> target, TKey key, TValue defaultValue)

    Tries to get the value by key. If key is not presented to dictionary returns ``defaultValue``.

.. function:: TValue AddOrUpdate<TKey, TValue>(IDictionary<TKey, TValue> target, TKey key, Func<TKey, TValue, TValue> updateFunc, TValue defaultValue)

    Adds a key/value pair to the ``IDictionary{TKey,TValue}`` if the key does not already exist, or updates a key/value pair in the ``IDictionary{TKey,TValue}`` if the key already exists. The default value will be used if key is missed to the ``IDictionary{TKey,TValue}``.

.. function:: string GetValueOrDefault(NameValueCollection target, string key, string defaultValues)

    Tries to get the value in ``NameValueCollection``. If value with specified key does not exist it will return ``defaultValue``. Can be used for application settings.

.. function:: string[] GetValuesOrDefault(NameValueCollection target, string key, string[] defaultValues)

    Tries to get the values in ``NameValueCollection``. If value with specified key does not exist it will return ``defaultValues``. Can be used for application settings.
