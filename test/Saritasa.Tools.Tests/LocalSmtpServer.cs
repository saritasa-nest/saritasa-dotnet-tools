// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Saritasa.Tools.Tests;

internal sealed class LocalSmtpServer : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
    private readonly ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    private readonly ConcurrentBag<Task> clientTasks = new ConcurrentBag<Task>();
    private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
    private readonly TaskCompletionSource<bool> firstAcceptedMessage =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> releaseFirstAcceptedMessage =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> rejectedRecipientSignal =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> releaseRejectedRecipient =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly bool delayFirstAcceptedMessage;
    private readonly bool delayRejectedResponse;
    private readonly string? rejectedRecipient;
    private readonly Task acceptTask;

    public LocalSmtpServer(
        bool delayFirstAcceptedMessage = false,
        string? rejectedRecipient = null,
        bool delayRejectedResponse = false)
    {
        this.delayFirstAcceptedMessage = delayFirstAcceptedMessage;
        this.rejectedRecipient = rejectedRecipient;
        this.delayRejectedResponse = delayRejectedResponse;

        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        acceptTask = AcceptClientsAsync();
    }

    public int Port { get; }

    public IReadOnlyList<string> Messages => messages.ToArray();

    public Task FirstAcceptedMessage => firstAcceptedMessage.Task;

    public Task RejectedRecipient => rejectedRecipientSignal.Task;

    public void ReleaseFirstAcceptedMessage()
    {
        releaseFirstAcceptedMessage.TrySetResult(true);
    }

    public void ReleaseRejectedRecipient()
    {
        releaseRejectedRecipient.TrySetResult(true);
    }

    private async Task AcceptClientsAsync()
    {
        try
        {
            while (!shutdown.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                clients.Add(client);
                clientTasks.Add(HandleClientAsync(client));
            }
        }
        catch (ObjectDisposedException) when (shutdown.IsCancellationRequested)
        {
        }
        catch (SocketException) when (shutdown.IsCancellationRequested)
        {
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true))
            using (var writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen: true)
            {
                AutoFlush = true,
                NewLine = "\r\n"
            })
            {
                await writer.WriteLineAsync("220 localhost").ConfigureAwait(false);

                bool rejectCurrentMessage = false;
                string? line;
                while (!shutdown.IsCancellationRequested && (line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250 localhost").ConfigureAwait(false);
                    }
                    else if (line.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase))
                    {
                        rejectCurrentMessage = false;
                        await writer.WriteLineAsync("250 sender accepted").ConfigureAwait(false);
                    }
                    else if (line.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
                    {
                        rejectCurrentMessage = rejectedRecipient != null &&
                            line.Contains(rejectedRecipient, StringComparison.OrdinalIgnoreCase);
                        if (rejectCurrentMessage)
                        {
                            rejectedRecipientSignal.TrySetResult(true);
                            if (delayRejectedResponse)
                            {
                                await releaseRejectedRecipient.Task.ConfigureAwait(false);
                            }

                            await writer.WriteLineAsync("550 recipient rejected").ConfigureAwait(false);
                        }
                        else
                        {
                            await writer.WriteLineAsync("250 recipient accepted").ConfigureAwait(false);
                        }
                    }
                    else if (line.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        if (rejectCurrentMessage)
                        {
                            await writer.WriteLineAsync("554 message rejected").ConfigureAwait(false);
                            continue;
                        }

                        await writer.WriteLineAsync("354 end with <CRLF>.<CRLF>").ConfigureAwait(false);
                        var message = new StringBuilder();
                        string? dataLine;
                        while ((dataLine = await reader.ReadLineAsync().ConfigureAwait(false)) != null && dataLine != ".")
                        {
                            if (dataLine.StartsWith("..", StringComparison.Ordinal))
                            {
                                dataLine = dataLine.Substring(1);
                            }

                            message.AppendLine(dataLine);
                        }

                        if (dataLine == null)
                        {
                            return;
                        }

                        messages.Enqueue(message.ToString());
                        if (delayFirstAcceptedMessage && messages.Count == 1)
                        {
                            firstAcceptedMessage.TrySetResult(true);
                            await releaseFirstAcceptedMessage.Task.ConfigureAwait(false);
                        }

                        await writer.WriteLineAsync("250 message accepted").ConfigureAwait(false);
                    }
                    else if (line.StartsWith("RSET", StringComparison.OrdinalIgnoreCase))
                    {
                        rejectCurrentMessage = false;
                        await writer.WriteLineAsync("250 reset").ConfigureAwait(false);
                    }
                    else if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("221 closing connection").ConfigureAwait(false);
                        return;
                    }
                    else
                    {
                        await writer.WriteLineAsync("250 command accepted").ConfigureAwait(false);
                    }
                }
            }
        }
        catch (IOException) when (shutdown.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (shutdown.IsCancellationRequested)
        {
        }
        catch (SocketException) when (shutdown.IsCancellationRequested)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        shutdown.Cancel();
        releaseFirstAcceptedMessage.TrySetResult(true);
        releaseRejectedRecipient.TrySetResult(true);
        listener.Stop();

        foreach (var client in clients)
        {
            client.Dispose();
        }

        await acceptTask.ConfigureAwait(false);
        await Task.WhenAll(clientTasks.ToArray()).ConfigureAwait(false);
        shutdown.Dispose();
    }
}
