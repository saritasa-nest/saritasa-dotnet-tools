Enum
====

.. function:: TAttribute GetAttribute<TAttribute>(Enum target) where TAttribute : Attribute

    Returns attribute relate to enum value.

.. function:: String GetDescription(Enum target)

    Returns the value of ``DescriptionAttribute`` attribute. If it is not presented it returns original "prettified" enum value, for example ``InProgress`` will be converted to ``In Progress``.
