using Newtonsoft.Json;
using NinjAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace NinjAPI.Results
{
    public class ResultHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Step 1: wait for response
            var response = await base.SendAsync(request, cancellationToken);

            return BuildApiResponse(request, response);
        }

        private HttpResponseMessage BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            // If we are trying to return a file ignore the wrap
            if (response.Content?.Headers.ContentDisposition?.DispositionType == "attachment") return response;

            List<Error> errors = new List<Error>();

            //Step 2: Extract response content, looking for errors
            if (response.TryGetContentValue(out object content) && !response.IsSuccessStatusCode)
            {
                if (content is HttpError error)
                {
                    // Step 2.1: If there are errors in content set null, so we return no content
                    content = null;

                    // Step 2.2: Add ModelState errors to response or server errors      
                    if (error.ModelState != null)
                    {
                        // read as string
                        var httpErrorObject = response.Content.ReadAsStringAsync().Result;

                        // Make anonymous object
                        var anonymousErrorObject = new { message = "", ModelState = new Dictionary<string, string[]>() };

                        // Deserialize anonymous type
                        var deserializedErrorObject = JsonConvert.DeserializeAnonymousType(httpErrorObject, anonymousErrorObject);

                        // Get modelState errors
                        var modelStateValues = deserializedErrorObject.ModelState.Select(kvp => KeyValuePair.Create(kvp.Key, string.Join(". ", kvp.Value))).ToList();

                        errors.Add(new Error(
                            Code: (int)response.StatusCode,
                            Name: Enum.GetName(typeof(HttpStatusCode), response.StatusCode),
                            Details: modelStateValues
                            ));
                    }
                    else if (error.Count > 0)
                    {
                        errors.Add(new Error(
                            Code: (int)response.StatusCode,
                            Name: Enum.GetName(typeof(HttpStatusCode), response.StatusCode),
                            Details: error.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.ToString())).ToList()
                            ));
                    }
                }
            }

            // Step 3: Make new wrapped response
            var newResponse = request.CreateResponse(response.StatusCode, response.IsSuccessStatusCode ? content : new ErrorResult(errors));

            // Step 4: Set headers to new response
            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }
    }
}
