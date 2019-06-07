// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// String helpers.
    /// </summary>
    internal static class StringHelpers
    {
        /// <remarks>
        /// See source at https://www.codeproject.com/Tips/823670/Csharp-Light-and-Fast-CSV-Parser .
        /// </remarks>
        public static string[] GetFieldsFromLine(string target, char delimiter = ',')
        {
            var inQuote = false;
            var records = new List<string>();
            var sb = new StringBuilder();
            var reader = new StringReader(target);

            while (reader.Peek() != -1)
            {
                var readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                    {
                        reader.Read();
                    }

                    if (inQuote)
                    {
                        if (readChar == '\r')
                        {
                            sb.Append('\r');
                        }
                        sb.Append('\n');
                    }
                    else
                    {
                        if (records.Count > 0 || sb.Length > 0)
                        {
                            records.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == '"')
                    {
                        inQuote = true;
                    }
                    else if (readChar == delimiter)
                    {
                        records.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace.
                    }
                    else
                    {
                        sb.Append(readChar);
                    }
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                    {
                        sb.Append(delimiter);
                    }
                    else
                    {
                        records.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (readChar == '"')
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == '"')
                        {
                            reader.Read();
                            sb.Append('"');
                        }
                        else
                        {
                            inQuote = false;
                        }
                    }
                    else
                    {
                        sb.Append(readChar);
                    }
                }
                else
                {
                    sb.Append(readChar);
                }
            }

            if (records.Count > 0 || sb.Length > 0)
            {
                records.Add(sb.ToString());
            }

            return records.ToArray();
        }

        public static IList<string> Tokenize(string target, char[] tokens)
        {
            var inQuote = false;
            var records = new List<string>();
            var sb = new StringBuilder();
            var reader = new StringReader(target);

            while (reader.Peek() != -1)
            {
                var readChar = (char)reader.Read();

                if (readChar == ' ' || readChar == '\r' && (char)reader.Peek() == '\n' ||
                    readChar == '\n')
                {
                    if (!inQuote)
                    {
                        if (sb.Length > 0)
                        {
                            records.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else
                    {
                        sb.Append(readChar);
                    }
                }
                else if (!inQuote && readChar == '\'' && (char)reader.Peek() == '\'' ||
                         !inQuote && readChar == '"' && (char)reader.Peek() == '"')
                {
                    sb.Append(readChar);
                    reader.Read();
                }
                else if (readChar == '\'' || readChar == '"')
                {
                    inQuote = !inQuote;
                }
                else if (!inQuote && tokens.Contains(readChar))
                {
                    if (sb.Length > 0)
                    {
                        records.Add(sb.ToString());
                        sb.Clear();
                    }
                    records.Add(readChar.ToString());
                }
                else
                {
                    sb.Append(readChar);
                }
            }

            if (sb.Length > 0)
            {
                records.Add(sb.ToString());
            }

            return records;
        }
    }
}
