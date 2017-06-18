DateTime
========

.. function:: DateTime Truncate(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)

    Truncates date by specified period. Begin of period.

.. function:: int CompareTo(DateTime target, DateTime value, DateTimePeriod period)

    The same as ``DateTime.CompareTo`` method but with additional truncation.

.. function:: IEnumerable<DateTime> GetRange(DateTime fromDate, DateTime toDate)

    Returns dates range. Uses lazy implementation.

.. function:: DateTime CombineDateAndTime(DateTime date, DateTime time)

    Combines date part from ``date`` and time from another.

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
