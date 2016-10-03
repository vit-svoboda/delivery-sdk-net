﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeliverSDK
{
    public class DeliverClient
    {
        private readonly HttpClient httpClient;
        private readonly DeliverUrlBuilder urlBuilder;


        /// <summary>
        /// Constructor for production API.
        /// </summary>
        /// <param name="projectId">Project ID.</param>
        public DeliverClient(string projectId)
        {
            urlBuilder = new DeliverUrlBuilder(projectId);
            httpClient = new HttpClient();
        }


        /// <summary>
        /// Constructor for preview API.
        /// </summary>
        /// <param name="projectId">Project ID.</param>
        /// <param name="accessToken">Preview access token.</param>
        public DeliverClient(string projectId, string accessToken)
        {
            urlBuilder = new DeliverUrlBuilder(projectId, accessToken);
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
        }


        /// <summary>
        /// Gets one content item by its codename.
        /// </summary>
        /// <param name="itemCodename">Item codename.</param>
        /// <param name="queryParams">Query parameters. For example: "elements=title" or "depth=0"."</param>
        public async Task<JObject> GetContentItemAsync(string itemCodename, params string[] queryParams)
        {
            var url = urlBuilder.GetUrlEndpoint(itemCodename, queryParams);
            return await GetDeliverResponseAsync(url);
        }


        /// <summary>
        /// Searches the content repository for items that match the criteria.
        /// </summary>
        /// <param name="queryParams">Query parameters. For example: "elements=title" or "depth=0"."</param>
        public async Task<JObject> GetContentItemsAsync(params string[] queryParams)
        {
            var url = urlBuilder.GetUrlEndpoint(null, queryParams);
            return await GetDeliverResponseAsync(url);
        }


        /// <summary>
        /// Gets one content item by its codename.
        /// </summary>
        /// <param name="itemCodename">Item codename.</param>
        /// <param name="parameters">Query parameters.</param>
        public async Task<JObject> GetContentItemAsync(string itemCodename, IEnumerable<IFilter> parameters = null)
        {
            var url = urlBuilder.ComposeDeliverUrl(itemCodename, parameters);
            return await GetDeliverResponseAsync(url);
        }


        /// <summary>
        /// Searches the content repository for items that match the filter criteria.
        /// </summary>
        /// <param name="parameters">Query parameters.</param>
        public async Task<JObject> GetContentItemsAsync(IEnumerable<IFilter> parameters = null)
        {
            var url = urlBuilder.ComposeDeliverUrl(filters: parameters);
            return await GetDeliverResponseAsync(url);
        }


        private async Task<JObject> GetDeliverResponseAsync(string url)
        {
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                return JObject.Parse(responseBody);
            }

            throw new DeliverException((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
