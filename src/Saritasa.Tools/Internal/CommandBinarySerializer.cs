// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Commands;

    internal class CommandBinarySerializer
    {
        const byte TokenBeginOfCommand = 0x10;
        const byte TokenId = 0x11;
        const byte TokenCommandType = 0x12;
        const byte TokenCommandName = 0x13;
        const byte TokenCommand = 0x14;
        const byte TokenData = 0x15;
        const byte TokenCreated = 0x16;
        const byte TokenExecutionDuration = 0x17;
        const byte TokenStatus = 0x18;
        const byte TokenErrorType = 0x19;
        const byte TokenError = 0x20;
        const byte TokenEndOfCommand = 0x50;

        static readonly Tuple<byte, byte[]> NullChunk = new Tuple<byte, byte[]>(0, null);

        static readonly byte[] EmptyBytes = new byte[] { };

        private readonly IObjectSerializer serializer;

        private readonly Stream stream;

        private readonly object objLock = new object();

        public CommandBinarySerializer(Stream stream, IObjectSerializer serializer)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            this.serializer = serializer;
            this.stream = stream;
        }

        private void WriteChunk(byte chunk, byte[] bytes = null)
        {
            stream.WriteByte(chunk);
            if (bytes != null)
            {
                stream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private Tuple<byte, byte[]> ReadChunk()
        {
            var header = new byte[1 + sizeof(int)];
            var n = stream.Read(header, 0, 1);
            if (header[0] == TokenBeginOfCommand || header[0] == TokenEndOfCommand)
            {
                return new Tuple<byte, byte[]>(header[0], null);
            }
            if (n == 0)
            {
                return NullChunk;
            }
            stream.Read(header, 1, sizeof(int));
            var length = BitConverter.ToInt32(header, 1);
            var content = new byte[length];
            stream.Read(content, 0, content.Length);
            return new Tuple<byte, byte[]>(header[0], content);
        }

        /// <summary>
        /// Reads the next command from stream.
        /// </summary>
        /// <returns>Command execution result.</returns>
        public CommandExecutionResult Read()
        {
            var result = new CommandExecutionResult();
            Tuple<byte, byte[]> chunk;
            Type commandType = null;
            Type errorType = null;
            bool commandStarted = false;
            while ((chunk = ReadChunk()) != NullChunk)
            {
                if (chunk.Item1 == TokenBeginOfCommand)
                {
                    commandStarted = true;
                }
                if (!commandStarted)
                {
                    continue;
                }

                switch (chunk.Item1)
                {
                    case TokenId:
                        result.CommandId = new Guid(chunk.Item2);
                        break;
                    case TokenCommandType:
                        commandType = Type.GetType(Encoding.UTF8.GetString(chunk.Item2));
                        break;
                    case TokenCommand:
                        result.Command = serializer.Deserialize(chunk.Item2, commandType);
                        break;
                    case TokenCommandName:
                        result.CommandName = Encoding.UTF8.GetString(chunk.Item2);
                        break;
                    case TokenData:
                        result.Data = (IDictionary<string, string>)serializer.Deserialize(chunk.Item2, typeof(IDictionary<string, string>));
                        break;
                    case TokenCreated:
                        result.CreatedAt = DateTime.FromBinary(BitConverter.ToInt64(chunk.Item2, 0));
                        break;
                    case TokenExecutionDuration:
                        result.ExecutionDuration = BitConverter.ToInt64(chunk.Item2, 0);
                        break;
                    case TokenErrorType:
                        errorType = Type.GetType(Encoding.UTF8.GetString(chunk.Item2));
                        break;
                    case TokenError:
                        result.Error = serializer.Deserialize(chunk.Item2, errorType) as Exception;
                        break;
                    case TokenStatus:
                        result.Status = (CommandExecutionContext.CommandStatus)chunk.Item2[0];
                        break;
                    case TokenEndOfCommand:
                        return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Writes the command to stream.
        /// </summary>
        /// <param name="result">Command execution result.</param>
        public void Write(CommandExecutionResult result)
        {
            var commandBytes = serializer.Serialize(result.Command);
            var errorBytes = result.Error != null ? serializer.Serialize(result.Error) : EmptyBytes;
            var dataBytes = result.Data != null ? serializer.Serialize(result.Data) : EmptyBytes;

            lock (objLock)
            {
                WriteChunk(TokenBeginOfCommand);
                WriteChunk(TokenId, result.CommandId.ToByteArray()); // id
                WriteChunk(TokenCommandType, Encoding.UTF8.GetBytes(result.Command.GetType().AssemblyQualifiedName)); // command type
                WriteChunk(TokenCommandName, Encoding.UTF8.GetBytes(result.CommandName)); // command name
                WriteChunk(TokenCreated, BitConverter.GetBytes(result.CreatedAt.ToBinary())); // created
                WriteChunk(TokenExecutionDuration, BitConverter.GetBytes(result.ExecutionDuration)); // completed
                WriteChunk(TokenStatus, BitConverter.GetBytes((byte)result.Status)); // status
                if (result.Error != null)
                {
                    WriteChunk(TokenErrorType, Encoding.UTF8.GetBytes(result.Error.GetType().AssemblyQualifiedName)); // error type
                    WriteChunk(TokenError, errorBytes); // error
                }
                WriteChunk(TokenCommand, commandBytes); // command object
                if (result.Data != null)
                {
                    WriteChunk(TokenData, dataBytes);
                }
                WriteChunk(TokenEndOfCommand);
            }
        }
    }
}
