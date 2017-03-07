SecurityUtils
=============

Contains set of methods based on standard library implementation to work with strings:

.. function:: string MD5(string target)

    Produces 128-bit hash value of string. PHP-compliant. The security of the MD5 hash function is severely compromised. It is not recommended for password hashing and provided only for backward compatibility.

.. function:: string Sha1(string target)

    Produces 160-bit hash value of string. The most widely used hasing algorithm. It is not recommended to use it for hashing now:

    .. https://community.qualys.com/blogs/securitylabs/2014/09/09/sha1-deprecation-what-you-need-to-know

.. function:: string Sha256(string target)

    Produces 256-bit hash value of string. Variant of SHA-2. It provides good security for password hashing.

.. function:: string Sha384(string target)

    Produces 384-bit hash value of string. Variant of SHA-2. It provides good security for password hashing.

.. function:: string Sha512(string target)

    Produces 512-bit hash value of string. Variant of SHA-2. It provides very good security for password hashing.

.. function:: UInt64 Crc32(string target)

    Returns CSC32 hash of string. A cyclic redundancy check (CRC) is an error-detecting code commonly used in digital networks and storage devices to detect accidental changes to raw data. Provides good hashing performance. Must not be used for sensitive data hashing (passwords, tokens, etc).

.. function:: string Hash(string target, HashMethods method)

    Hash string with selected hash method. The string will contain method name that was used for hashing. Possible methods are ``Md5``, ``Sha1``, ``Sha256``, ``Sha384``, ``Sha512``.

.. function:: bool CheckHash(string target, string hashedStrToCheck)

    Compares target string with hashed string.
