// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// String helpers.
    /// </summary>
    internal static class StringHelpers
    {
        /// <remarks>
        /// Source: https://www.codeproject.com/Tips/823670/Csharp-Light-and-Fast-CSV-Parser
        /// </remarks>
        public static string[] GetFieldsFromLine(string line, char delimiter = ',')
        {
            var inQuote = false;
            var record = new List<string>();
            var sb = new StringBuilder();
            var reader = new StringReader(line);

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
                        if (record.Count > 0 || sb.Length > 0)
                        {
                            record.Add(sb.ToString());
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
                        record.Add(sb.ToString());
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
                        record.Add(sb.ToString());
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

            if (record.Count > 0 || sb.Length > 0)
            {
                record.Add(sb.ToString());
            }

            return record.ToArray();
        }
    }
}
