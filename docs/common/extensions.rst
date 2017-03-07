Extensions
==========

The Saritasa.Tools.Extensions namespace contains extension methods for various classes.

CollectionsExtensions
---------------------

.. class:: CollectionsExtensions

    Set of extensions related to collections (`IEnumerable`, `IList`, etc).

    .. function:: static IOrderedEnumerable<TSource> Order<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, SortOrder sortOrder)

        Extension to order enumerable by asc or desc order. ``Ask`` and ``Desc`` are ``SortOrder`` enum members.

    .. function:: static Common.PagedEnumerable<T> GetPaged<T>(IEnumerable<T> source, int page, int pageSize)

        Get paged enumeration. See ``PagedEnumerable`` class for more reference.

    .. function:: IEnumerable<IEnumerable<T>> ChunkSelectRange<T>(IEnumerable<T> source, int chunkSize)

        Breaks a list of items into chunks of a specific size and yeilds T items. Default ``chunkSize`` is 1000.

    .. function:: IEnumerable<T> ChunkSelect<T>(IEnumerable<T> source, int chunkSize)

        Breaks a list of items into chunks of a specific size and yeilds T items. Default ``chunkSize`` is 1000.

    .. function:: void ForEach<T>(IEnumerable<T> target, Action<T> action)

        Implements foreach loop with Action. Action does something with each item of collection. Since there is a tacit agreement that linq extensions should not change collection items it is implemented as helper method. Default chunk size is 1000. For example you can use it like this:

            .. code-block:: c#

                foreach (var user in Users) {
                    user.FirstName = StringExtensions.Capitalize(user.FirstName);
                }

                // can be replaced

                Users.ForEach(u => { u.FirstName = StringExtensions.Capitalize(u.FirstName) });

    .. function:: int FirstIndexMatch<T>(IEnumerable<T> target, Predicate<T> condition)

         Returns item index in enumerable that matches specific condition. Example:

            .. code-block:: c#

                var arr = new[] { 10, 45, 6, 34, 6 };
                var index = arr.FirstIndexMatch(a => a == 6); // returns 2

    .. function:: IEnumerable<TSource> Distinct<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)

        Returns distinct elements from a sequence by using the key selector to compare values.

DictionaryExtensions
--------------------

.. class:: DictionaryExtensions

    Set of extensions related to `IDictionary`.

    .. function:: TValue GetValueDefault<TKey, TValue>(IDictionary<TKey, TValue> target, TKey key, TValue defaultValue)

        Tries to get the value by key. If key is not presented to dictionary returns ``defaultValue``.

EnumExtensions
--------------

.. class:: EnumExtensions

    .. function:: String GetDescription(Enum target)

        Returns the value of DescriptionAttribute attribute.

StringExtensions
----------------

.. class:: StringExtensions

    These are extension methods that applied to String type.

    .. function:: String.FormatWith(params object[] args)

        With this extension you can easly append parameters to any string.

            .. code-block:: c#

                // without extensions:
                Console.WriteLine(String.Format("The sum of {1} and {2} is {3}", a, b, sum));

                // with extension:
                Console.WriteLine("The sum of {1} and {2} is {3}".FormatWith(a, b, sum));

    .. function:: Boolean String.IsEmpty()

        Returns true if string is empty. Without extensions you have to write ``String.IsNullOrEmpty(str)``.

    .. function:: Boolean String.IsNotEmpty()

        Returns true if string is not empty. Without extensions you have to write ``!String.IsNullOrEmpty(str)``.

    .. function:: String String.NullSafe()

        Returns empty string if target string is empty or string itself. It is the same as ``(mystring ?? "")``.

DateTimeExtensions
------------------

.. class:: DateTimeExtensions

    .. function:: Boolean IsHoliday(DateTime target)

        Just checkes is this a Saturday or Sunday.

    .. function:: DateTime Truncate(DateTime target, DateTimeTruncation truncation)

        Trancates the date by seconds, minutes, hours, days or months.

    .. function:: bool IsBetween(DateTime target, DateTime startDate, DateTime endDate)

        Is target date between startDate and endDate dates.

    .. function:: int CompareTo(DateTime target, DateTime value, DateTimePeriod period)

        The same as DateTime.CompareTo method but with additional truncation.

    .. function:: IEnumerable<DateTime> Range(DateTime fromDate, DateTime toDate)

        Returns dates range.

    .. function:: DateTime CombineDateTime(DateTime date, DateTime time)

        Combines date part from first date and time from another.

    .. function:: DateTime StartOf(DateTime target, DateTimePeriod period)

        Start datetime of period.

    .. function:: DateTime EndOf(DateTime target, DateTimePeriod period)

        End datetime of period.

    .. function:: DateTime FromUnixTimestamp(double unixTimeStamp)

        Converts from unix time stamp to DateTime.

    .. function:: double ToUnixTimestamp(DateTime target)

        Converts DateTime to unix time stamp.

    .. function:: double Diff(DateTime target1, DateTime target2, DateTimePeriod period)

        Return period difference between two dates. Negative values are converted to positive.

    .. function:: DateTime Set(DateTime target, DateTimePeriod period, int value)

        Shortcut to set date part.
