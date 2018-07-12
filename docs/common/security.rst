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

.. function:: byte[] Pbkdf2Sha1(string target, int numOfIterations)
.. function:: byte[] Pbkdf2Sha1(string target, byte[] salt, int numOfIterations)

    Returns PBKDF2 compatible hash by using a pseudo-random number generator based on HMACSHA1. Default number of iterations is 10000.

.. function:: string Hash(string target, HashMethods method)

    Hash string with selected hash method. The string will contain method name that was used for hashing. Possible methods are ``Md5``, ``Sha1``, ``Sha256``, ``Sha384``, ``Sha512``, ``Pbkdf2Sha1``.

.. function:: bool CheckHash(string target, string hashedStringToCheck)

    Compares target string with hashed string.

.. function:: uint Crc32(byte[] target, uint polynomial, uint seedCrc)

    Returns CRC-32 checksum. By default uses IEEE polynomial. Possible polynomials to use:

        - ``Crc32Ieee`` *(default)*
        - ``Crc32Castagnoli``
        - ``Crc32Koopman``

.. function:: ulong Crc64(byte[] target, ulong polynomial, ulong seedCrc)

    Returns the CRC-64 checksum of data using the polynomial. Default implementation uses ISO polynomial. Possible polynomials to use:

        - ``Crc64IsoPolynomial`` *(default)*
        - ``Crc64EcmaPolynomial``

.. function:: string ConvertBytesToString(byte[] bytes)

    Convert array of bytes to string representation. Replace dashes by empty strings.

.. function:: byte[] ConvertStringToBytes(string target)

    Convert string that contains hex representation of bytes to bytes array.
