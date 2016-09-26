// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Mvc.Utils
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Adds cors headers to response.
    /// </summary>
    public class CorsDelegatingHandler : DelegatingHandler
    {
        const string Origin = "Origin";
        const string AccessControlRequestMethod = "Access-Control-Request-Method";
        const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isCorsRequest = request.Headers.Contains(Origin);
            bool isPreflightRequest = request.Method == HttpMethod.Options;
            if (isCorsRequest)
            {
                if (isPreflightRequest)
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

                        string accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
                        if (accessControlRequestMethod != null)
                        {
                            response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
                        }

                        string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
                        if (!string.IsNullOrEmpty(requestedHeaders))
                        {
                            response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
                        }

                        return response;
                    },
                    cancellationToken);
                }
                else
                {
                    return base.SendAsync(request, cancellationToken).ContinueWith(t =>
                    {
                        HttpResponseMessage response = t.Result;
                        response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                        return response;
                    }, cancellationToken);
                }
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
