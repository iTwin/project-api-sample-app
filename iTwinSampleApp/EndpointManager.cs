/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using ItwinProjectSampleApp.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ItwinProjectSampleApp
    {
    internal class EndpointManager
        {
        private static readonly HttpClient client = new();
        private const string API_BASE_URL = "https://api.bentley.com";

        #region Constructors
        internal EndpointManager (string token)
            {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.bentley.itwin-platform.v1+json");
            client.DefaultRequestHeaders.Add("Authorization", token);
            }
        #endregion

        internal async Task<HttpGetResponseMessage<T>> MakeGetCall<T> (string relativeUrl, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(client, customHeaders);

            // Construct full url and then make the GET call
            using var response = await client.GetAsync($"{API_BASE_URL}{relativeUrl}");

            if ( response.StatusCode == HttpStatusCode.TooManyRequests )
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpGetResponseMessage.
            HttpGetResponseMessage<T> responseMsg = new HttpGetResponseMessage<T>();
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseMsg.Content);
            if ( response.StatusCode == HttpStatusCode.OK )
                {
                // Successful response. Deserialize the list of objects returned.
                var containerName = $"{typeof(T).Name.ToLower()}s"; // The container is plural for lists
                var instances = responsePayload[containerName];
                responseMsg.Instances = new List<T>();
                foreach ( var inst in instances )
                    {
                    responseMsg.Instances.Add(inst.ToObject<T>());
                    }
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpGetSingleResponseMessage<T>> MakeGetSingleCall<T> (string relativeUrl, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(client, customHeaders);

            // Construct full url and then make the GET call
            using var response = await client.GetAsync($"{API_BASE_URL}{relativeUrl}");

            if ( response.StatusCode == HttpStatusCode.TooManyRequests )
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpGetSingleResponseMessage.
            HttpGetSingleResponseMessage<T> responseMsg = new HttpGetSingleResponseMessage<T>();
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseMsg.Content);
            if ( response.StatusCode == HttpStatusCode.OK )
                {
                // Successful response. Deserialize the object returned.
                var containerName = typeof(T).Name.ToLower();
                responseMsg.Instance = responsePayload[containerName].ToObject<T>();
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpPostResponseMessage<T>> MakePostCall<T> (string relativeUrl, T propertyModel, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(client, customHeaders);

            var body = new StringContent(JsonSerializer.Serialize(propertyModel, JsonSerializerOptions), Encoding.UTF8, "application/json");
            HttpPostResponseMessage<T> responseMsg = new HttpPostResponseMessage<T>();

            // Construct full url and then make the POST call
            using (var response = await client.PostAsync($"{API_BASE_URL}{relativeUrl}", body))
                {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                    // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                    }

                // Copy/Deserialize the response into custom HttpPostResponseMessage.

                responseMsg.Status = response.StatusCode;
                responseMsg.Content = await response.Content.ReadAsStringAsync();
                }

            if (!string.IsNullOrEmpty(responseMsg.Content))
                {
                var responsePayload = JObject.Parse(responseMsg.Content);
                if (responseMsg.Status == HttpStatusCode.Created)
                    {
                    // Successful response. Deserialize the object returned. This is the full representation
                    // of the new instance that was just created. It will contain the new instance Id.
                    var containerName = typeof(T).Name.ToLower();

                    responseMsg.NewInstance = responsePayload[containerName].ToObject<T>();
                    }
                else
                    {
                    // There was an error. Deserialize the error details and return.
                    responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                    }
                }
            else
                responseMsg.NewInstance = propertyModel;


            return responseMsg;
            }

        internal async Task<HttpPostResponseMessage<T>> MakePostCall<T> (string relativeUrl, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(client, customHeaders);

            // Construct full url and then make the POST call
            using var response = await client.PostAsync($"{API_BASE_URL}{relativeUrl}", null);

            if ( response.StatusCode == HttpStatusCode.TooManyRequests )
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpPostResponseMessage.
            HttpPostResponseMessage<T> responseMsg = new HttpPostResponseMessage<T>();
            responseMsg.Status = response.StatusCode;

            if ( response.StatusCode == HttpStatusCode.OK )
                {
                // There was no payload and no expected response to return.
                responseMsg.NewInstance = default(T);
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.Content = await response.Content.ReadAsStringAsync();
                var responsePayload = JObject.Parse(responseMsg.Content);
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpPatchResponseMessage<T>> MakePatchCall<T> (string relativeUrl, object patchedObject, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(client, customHeaders);

            // Construct full url and then make the PATCH call
            using var response = await client.PatchAsync($"{API_BASE_URL}{relativeUrl}",
                new StringContent(JsonSerializer.Serialize(patchedObject, JsonSerializerOptions), Encoding.UTF8, "application/json-patch+json"));
            if ( response.StatusCode == HttpStatusCode.TooManyRequests )
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpPatchResponseMessage.
            HttpPatchResponseMessage<T> responseMsg = new HttpPatchResponseMessage<T>();
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseMsg.Content);
            if ( response.StatusCode == HttpStatusCode.OK )
                {
                // Successful response. Deserialize the object returned. This is the full representation
                // of the instance that was just updated, including the updated values.
                var containerName = typeof(T).Name.ToLower();
                responseMsg.UpdatedInstance = responsePayload[containerName].ToObject<T>();
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpResponseMessage<T>> MakeDeleteCall<T> (string relativeUrl, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(client, customHeaders);

            // Construct full url and then make the POST call
            using var response = await client.DeleteAsync($"{API_BASE_URL}{relativeUrl}");
            if ( response.StatusCode == HttpStatusCode.TooManyRequests )
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpResponseMessage.
            HttpResponseMessage<T> responseMsg = new HttpResponseMessage<T>();
            responseMsg.Status = response.StatusCode;
            if ( response.StatusCode != HttpStatusCode.NoContent )
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.Content = await response.Content.ReadAsStringAsync();
                var responsePayload = JObject.Parse(responseMsg.Content);
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        #region Private Methods

        private void AddCustomHeaders (HttpClient client, Dictionary<string, string> customHeaders = null)
            {
            if ( customHeaders != null )
                {
                foreach ( var ch in customHeaders )
                    {
                    client.DefaultRequestHeaders.Add(ch.Key, ch.Value);
                    }
                }
            }
        private static JsonSerializerOptions JsonSerializerOptions
            {
            get
                {
                var options = new JsonSerializerOptions
                    {
                    IgnoreNullValues = true,
                    WriteIndented = true,
                    AllowTrailingCommas = false,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    Converters = {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                    };
                return options;
                }
            }
        #endregion
        }

    #region Supporting Classes
    internal class HttpResponseMessage<T>
        {
        public HttpStatusCode Status
            {
            get; set;
            }
        public string Content
            {
            get; set;
            }
        public ErrorDetails ErrorDetails
            {
            get; set;
            }
        }

    internal class HttpPostResponseMessage<T> : HttpResponseMessage<T>
        {
        public T NewInstance
            {
            get; set;
            }
        }
    internal class HttpPatchResponseMessage<T> : HttpResponseMessage<T>
        {
        public T UpdatedInstance
            {
            get; set;
            }
        }
    internal class HttpGetResponseMessage<T> : HttpResponseMessage<T>
        {
        public List<T> Instances
            {
            get; set;
            }
        }
    internal class HttpGetSingleResponseMessage<T> : HttpResponseMessage<T>
        {
        public T Instance
            {
            get; set;
            }
        }
    #endregion
    }
