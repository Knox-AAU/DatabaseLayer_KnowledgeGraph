using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RDFApi
{
    public sealed class VirtuosoDataStore
    {
        private readonly string endpoint;

        public VirtuosoDataStore(string endpoint)
        {
            this.endpoint = endpoint;
        }
        
        public async Task<string> Query(string query)
        {
            UriBuilder uriBuilder = new(endpoint)
            {
                Query = $"query={query}"
            };

            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uriBuilder.ToString()),
            };
            
            HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                // Virtuoso sends errors in SPARQL through content, which we want to send to the users.
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"{response.StatusCode}: {error}");
            }
            
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}