DateTime
========

.. function:: DateTime Truncate(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)

    Truncates date by specified period. Return begin of period.

.. function:: int CompareTo(DateTime target, DateTime value, DateTimePeriod period)

    Compares the value of the instance to a specified ``DateTime`` object with truncation, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified ``DateTime`` value.

.. function:: IEnumerable<DateTime> GetRange(DateTime fromDate, DateTime toDate)

    Returns dates range. Uses lazy implementation.

.. function:: DateTime CombineDateAndTime(DateTime date, DateTime time)

    Combines date part from ``date`` and time from another. Kind is getting from time part.

.. function:: DateTime GetStartOfPeriod(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)

    Start datetime of period.

.. function:: DateTime GetEndOfPeriod(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)

    End datetime of period.

.. function:: DateTime FromUnixTimestamp(double unixTimeStamp)

    Converts from unix time stamp to ``DateTime``.

.. function:: double ToUnixTimestamp(DateTime target)

    Converts ``DateTime`` to unix time stamp.

.. function:: double GetDiff(DateTime target1, DateTime target2, DateTimePeriod period)

    Return period difference between two dates. Negative values are converted to positive.

.. function:: DateTime SetPart(DateTime target, DateTimePeriod period, int value)

    Shortcut to set date part.
