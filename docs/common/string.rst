StringUtils
===========

String utils.

.. function:: string SafeTruncate(string target, int maxLength)

    Truncates target string to max length. Useful to do not allow string to exceed specific amount of character.

.. function:: static string SafeSubstring(string target, int startIndex, int length)

    Retrieves a substring from this instance. If start index had negative value it will be replaced to 0. If substring exceed length of target string the end of string will be returned.

.. function:: static string NullSafe(string target)

    Returns empty string if target string is null or string itself. For those who wants clean syntax:

        .. code-block:: c#

            var str1 = str1 ?? string.Empty;
            var str2 = StringUtils.NullSage(str2);

.. function:: string JoinIgnoreEmpty(string separator, params string[] values)
              string JoinIgnoreEmpty(string separator, IEnumerable<string> values)

.. function:: ParseOrDefault

    Sometimes when we try to convert some type from string to another one (`int.Parse` for example) we don't need to know if is it possible to do that or not. Having default value in that case is good for us. This set of methods `TryParseXDefault` try to parse input value and if it is not possible return default one.

    .. code-block:: c#

        // with standard library:
        int val = 0;
        if (!int.TryParse("1q", out val))
            val = 1;

        // with Saritasa extensions:
        int val = StringUtils.ParseDefault("1q", 1);

    .. note:: There is extended behavior for boolean parse. Following values will be at ``True``: ``T``, ``t``, ``1``, ``Y``, ``y``. The values ``F``, ``f``, ``0``, ``N``, ``n`` will be parsed as ``False``.
