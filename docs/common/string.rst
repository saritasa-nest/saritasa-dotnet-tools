StringUtils
===========

String utils.

.. function:: string ConvertToSnakeCase(string target)

    Converts string to snake case string style. Example: HelloWorld -> hello_world.

.. function:: bool IsEmail(string target)

    Returns true if strign is email address. Uses ``CheckConstants.EmailExpression`` regexp to check.

.. function:: string Truncate(string target, int maxLength)

    Truncates target string to max length. Useful to do not allow string to exceed specific amount of character.

.. function:: string Join(String separator, IEnumerable<String> values)

    Concatenates enumerable of strings using the specified separator.

.. function:: string JoinIgnoreEmpty(string separator, params string[] values)
              string JoinIgnoreEmpty(string separator, IEnumerable<string> values)

.. function:: string WildcardToRegex(sring pattern)

    Converts wildcard characters to regexp string. For example `He*ll? -> He\*ll\?`.

.. function:: T TryParseEnumDefault<T>(string target, T defaultValue)

    Convert string value to enum value or return default

.. function:: T TryParseEnumDefault<T>(string target, bool ignoreCase, T defaultValue)

    Convert string value to enum value or return default.

.. function:: Parse

    Sometimes when we try to convert some type from string to another one (`int.Parse` for example) we don't need to know if is it possible to do that or not. Having default value in that case is good for us. This set of methods `TryParseXDefault` try to parse input value and if it is not possible return default one.

    .. code-block:: c#

        // with standard library:
        int val = 0;
        if (!int.TryParse("1q", out val))
            val = 1;

        // with Saritasa extensions:
        int val = StringUtils.ParseDefault("1q", 1);
