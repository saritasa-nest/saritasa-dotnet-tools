// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Messages.Endpoints
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
    /// Web endpoint.
    /// </summary>
    public class WebEndpoint : IMessageEndpoint, IDisposable
    {
        const string HttpVerbPost = "POST";
        const int DefaultConnectionWaitTime = 550; // ms
        const string ContentTypeJson = "application/json";
        const string ContentTypePlainText = "text/plain";
        const string Server = "Saritasa.Tools WebEndpoint/1.0.0";

        /// <summary>
        /// Default TCP port;
        /// </summary>
        public const int DefaultPort = 26025;

        IMessagePipeline[] pipelines;

        HttpListener listener;

        int port;

        string address;

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
            var thread = new Thread(new ThreadStart(Listen));
            thread.IsBackground = true;
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
                threadWaitEvent.Wait();
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

            using (HttpListenerResponse response = listenerContext.Response)
            {
                listenerContext.Response.AddHeader("Server", Server);

                if (request.HttpMethod == HttpVerbPost)
                {
                    response.ContentType = ContentTypeJson;
                    using (var streamReader = new StreamReader(request.InputStream))
                    {
                        Message message = null;
                        try
                        {
                            var body = streamReader.ReadToEnd();
                            message = JsonConvert.DeserializeObject<Message>(body);
                            TypeHelpers.ResolveTypeForContent(
                                message,
                                System.Text.Encoding.UTF8.GetBytes(message.Content.ToString()),
                                new ObjectSerializers.JsonObjectSerializer(), // only json is supported
                                AppDomain.CurrentDomain.GetAssemblies() // pretend that all types are loaded to current domain
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

        private void FormatStreamFromString(string input, HttpListenerResponse response)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(input);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();
        }

        private void ProcessMessage(Message message)
        {
            InternalLogger.Trace($"Processing message id: {message.Id} contenttype: {message.ContentType}",
                nameof(WebEndpoint));
            bool isPipelineFound = false;
            for (int i = 0; i < pipelines.Length; i++)
            {
                if (pipelines[i].MessageTypes.Contains(message.Type))
                {
                    pipelines[i].ProcessRaw(message);
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
            this.pipelines = pipelines;
        }

        #region Dispose

        private bool disposed = false;

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
        }

        #endregion
    }
}
#endif
