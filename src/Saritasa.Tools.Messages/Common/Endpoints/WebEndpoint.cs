// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Messages.Common.Endpoints
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Internal;

    /// <summary>
    /// Web endpoint. Allows to send messages directly to application. Must be POST request.
    /// Sample:
    /// POST http://localhost:26025/query/ZergRushCo.Todosya.Domain.Tasks.Queries.ProjectsQueries.GetByUser
    /// {"userId":2,"page":1,"pageSize":10}
    /// </summary>
    public class WebEndpoint : IMessageEndpoint, IDisposable
    {
        private const string HttpVerbPost = "POST";
        private const int DefaultConnectionWaitTime = 550; // ms
        private const string ContentTypeJson = "application/json";
        private const string ContentTypePlainText = "text/plain";
        private const string Server = "Saritasa.Tools WebEndpoint/1.0.0";

        /// <summary>
        /// Default TCP port;
        /// </summary>
        public const int DefaultPort = 26025;

        IMessagePipeline[] pipelines;

        HttpListener listener;

        readonly int port;

        readonly string address;

        CancellationToken cancellationToken = CancellationToken.None;

        CancellationTokenSource cancellationTokenSource;

        ManualResetEventSlim threadWaitEvent = new ManualResetEventSlim();

        /// <summary>
        /// .ctor
        /// <param name="address">Address to bind, loopback by default.</param>
        /// <param name="port">TCP port. By default 26025.</param>
        /// </summary>
        public WebEndpoint(string address = "127.0.0.1", int port = 26025)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            this.port = port;
            this.address = address;
            listener = new HttpListener();
            listener.Prefixes.Add($"http://{address}:{port}/");
        }

        /// <summary>
        /// Start tcp port listening.
        /// </summary>
        public void Start()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(WebEndpoint));
            }
            if (listener.IsListening)
            {
                return;
            }
            InternalLogger.Trace($"Web endpoint starting on {address}:{port}", nameof(WebEndpoint));

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            listener.Start();
            var thread = new Thread(Listen)
            {
                Name = "MessagesWebEndpoint",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            thread.Start();

            InternalLogger.Trace($"Web endpoint started on {address}:{port}", nameof(WebEndpoint));
        }

        /// <summary>
        /// Stop listening. Need to wait for last executing message.
        /// <param name="wait">Wait when all handlers will be stopped.</param>
        /// </summary>
        public void Stop(bool wait = false)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(WebEndpoint));
            }
            if (listener.IsListening == false || cancellationTokenSource == null)
            {
                return;
            }

            InternalLogger.Trace("Web endpoint stop requested", nameof(WebEndpoint));
            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
            if (wait)
            {
                threadWaitEvent.Wait(cancellationToken);
                threadWaitEvent.Reset();
            }
        }

        private void Listen()
        {
            while (true)
            {
                var taskContext = listener.GetContextAsync();
                taskContext.Wait(DefaultConnectionWaitTime);
                taskContext.ContinueWith(t =>
                {
                    var context = t.Result;
                    InternalLogger.Info($"Incoming request from {context.Request.UserHostAddress} {context.Request.HttpMethod}",
                        nameof(WebEndpoint));
                    try
                    {
                        HandleRequest(context);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error($"Error while executing message: {ex.ToString()}", nameof(WebEndpoint));
                    }
                }, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

                if (cancellationToken.IsCancellationRequested)
                {
                    listener.Stop();
                    break;
                }
            }

            InternalLogger.Trace("Web endpoint stopped", nameof(WebEndpoint));
            threadWaitEvent.Set();
        }

        private void HandleRequest(HttpListenerContext listenerContext)
        {
            HttpListenerRequest request = listenerContext.Request;

            using (var response = listenerContext.Response)
            {
                listenerContext.Response.AddHeader("Server", Server);

                if (request.HttpMethod == HttpVerbPost)
                {
                    var message = new Message();
                    message.Type = GetMessageTypeFromUri(request.Url);
                    message.ContentType = GetMessageContentTypeFromUri(request.Url);
                    message.CreatedAt = DateTime.Now;

                    response.ContentType = ContentTypeJson;
                    using (var streamReader = new StreamReader(request.InputStream))
                    {
                        try
                        {
                            var body = streamReader.ReadToEnd();
                            message.Content = JsonConvert.DeserializeObject(body);
                            TypeHelpers.ResolveTypeForContent(
                                message,
                                System.Text.Encoding.UTF8.GetBytes(message.Content.ToString()),
                                new ObjectSerializers.JsonObjectSerializer(), // only json is supported
                                AppDomain.CurrentDomain.GetAssemblies() // pretend that all types are loaded into current domain
                            );
                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = 400;
                            response.ContentType = ContentTypePlainText;
                            FormatStreamFromString(ex.ToString(), response);
                        }

                        try
                        {
                            ProcessMessage(message);
                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = 400;
                            response.ContentType = ContentTypePlainText;
                            FormatStreamFromString(ex.ToString(), response);
                        }
                        FormatStreamFromString(JsonConvert.SerializeObject(message), response);
                    }
                }
                else
                {
                    response.StatusCode = 405;
                    response.ContentType = ContentTypePlainText;
                    FormatStreamFromString("Method Not Allowed", response);
                }
            }
        }

        private static byte GetMessageTypeFromUri(Uri uri)
        {
            var str = uri.PathAndQuery.Trim();
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException(nameof(uri));
            }

            if (str[0] == '/')
            {
                str = str.Substring(1, str.Length - 1);
            }

            var ind = str.IndexOf(@"/", StringComparison.Ordinal);
            if (ind < 0)
            {
                throw new ArgumentException(nameof(uri));
            }

            var cmd = str.Substring(0, ind);
            byte cmdnum = 0;
            if (byte.TryParse(cmd, out cmdnum))
            {
                return cmdnum;
            }

            switch (cmd)
            {
                case "command": return Message.MessageTypeCommand;
                case "query": return Message.MessageTypeQuery;
                case "event": return Message.MessageTypeEvent;
            }

            throw new ArgumentException(nameof(uri));
        }

        static string GetMessageContentTypeFromUri(Uri uri)
        {
            var str = uri.PathAndQuery.Trim();
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException(nameof(uri));
            }

            if (str[0] == '/')
            {
                str = str.Substring(1, str.Length - 1);
            }

            var ind = str.IndexOf(@"/", StringComparison.Ordinal);
            if (ind < 0)
            {
                throw new ArgumentException(nameof(uri));
            }

            var contentType = str.Substring(ind + 1, uri.PathAndQuery.Length - ind - 2);
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return contentType;
        }

        private static void FormatStreamFromString(string input, HttpListenerResponse response)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(input);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();
        }

        private void ProcessMessage(Message message)
        {
            InternalLogger.Trace($"Processing message id: {message.Id} contenttype: {message.ContentType}",
                nameof(WebEndpoint));
            var isPipelineFound = false;
            foreach (IMessagePipeline pipeline in pipelines)
            {
                if (pipeline.MessageTypes.Contains(message.Type))
                {
                    pipeline.ProcessRaw(message);
                    isPipelineFound = true;
                }
            }

            if (!isPipelineFound)
            {
                InternalLogger.Warn($"Pipeline not found for message type {message.Type}" +
                    "id: {message.Id} contenttype: {message.ContentType}", nameof(WebEndpoint));
            }
        }

        /// <inheritdoc />
        public void RegisterPipelines(params IMessagePipeline[] pipelines)
        {
            if (pipelines.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(pipelines));
            }
            this.pipelines = pipelines;
        }

        #region Dispose

        bool disposed;

        /// <summary>
        /// Dispose object. Remove thread, stop listening.
        /// </summary>
        /// <param name="disposing">Dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Stop(true);

                if (disposing)
                {
                    if (listener != null)
                    {
                        listener.Close();
                        listener = null;
                    }
                    if (cancellationTokenSource != null)
                    {
                        cancellationTokenSource.Dispose();
                        cancellationTokenSource = null;
                    }
                    if (threadWaitEvent != null)
                    {
                        threadWaitEvent.Dispose();
                        threadWaitEvent = null;
                    }
                }
                disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
#endif
