using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

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
            UriBuilder uriBuilder = new(endpoint);

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("query", query)
            });
            
            HttpClient httpClient = new();
            var response = await httpClient.PostAsync(uriBuilder.ToString(), formContent);

            if (!response.IsSuccessStatusCode)
            {
                // Virtuoso sends errors in SPARQL through content, which we want to send to the users.
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"{response.StatusCode}: {error}");
            }
            
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> InsertTurtleGraph(string turtle)
        {

            Graph g = new();
            g.BaseUri = new Uri("http://docker.internal.host:1111/");
            GraphHandler graphHandler = new(g);
            TurtleParser parser = new();
            parser.Load(graphHandler, new StringReader(turtle));
            
            Console.WriteLine($"Triples in graph: {g.Triples.Count}");
            Console.WriteLine($"Accepts all: {graphHandler.AcceptsAll}");
            
            NTriplesFormatter formatter = new();
            StringBuilder sb = new();
            sb.Append("INSERT DATA {");
            foreach (Triple gTriple in g.Triples)
            {
                sb.Append(gTriple.ToString(formatter));
            }
            sb.Append("}");

            UnicodeEncoding encoding = new();
            byte[] bytes = encoding.GetBytes(sb.ToString());
            char[] chars = encoding.GetChars(bytes);
            string insertQuery = new(chars);
            
            return await Query(insertQuery);
        }
    }
}