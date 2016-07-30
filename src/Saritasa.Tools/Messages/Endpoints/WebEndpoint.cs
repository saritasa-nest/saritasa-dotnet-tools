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

    /// <summary>
    /// Web endpoint.
    /// </summary>
    public class WebEndpoint : IMessageEndpoint
    {
        IMessagePipeline[] pipelines;

        /// <summary>
        /// .ctor
        /// </summary>
        public WebEndpoint()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:9999/");
            listener.Start();
            Thread thread = new Thread(new ParameterizedThreadStart(Listen));
            thread.Start(listener);
        }

        private static void Listen(object obj)
        {
            HttpListener listener = (HttpListener)obj;
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "POST")
                {
                    var req = request.Url;
                    using (var streamReader = new StreamReader(request.InputStream))
                    {
                        var st = streamReader.ReadToEnd();
                    }
                }

                string responseString = "<html><head><meta charset='utf8'></head><body>Привет мир!</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        private void Process(Message message)
        {
            for (int i = 0; i < pipelines.Length; i++)
            {
                if (pipelines[i].MessageTypes.Contains(message.Type))
                {
                    pipelines[i].ProcessRaw(message);
                }
            }
        }

        /// <inheritdoc />
        public void RegisterPipelines(params IMessagePipeline[] pipelines)
        {
            this.pipelines = pipelines;
        }
    }
}
#endif
