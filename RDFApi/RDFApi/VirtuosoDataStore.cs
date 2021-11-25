using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace RDFApi
{
    public sealed class VirtuosoDataStore
    {
        private readonly string endpoint;
        private readonly Uri uri;

        public VirtuosoDataStore(string endpoint)
        {
            this.endpoint = endpoint;
            uri = new Uri(endpoint.Replace("sparql", ""));
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

        public async Task<string> InsertTurtleGraph(string turtle, string graphName = "knox")
        {
            IGraph g = new Graph()
            {
                BaseUri = uri,
            };
            
            GraphHandler graphHandler = new(g);
            TurtleParser parser = new();
            parser.Load(graphHandler, new StringReader(turtle));
            
            Console.WriteLine($"Triples in graph: {g.Triples.Count}");

            StringBuilder sb = new();
            
            sb.Append("INSERT DATA { GRAPH <{" + graphName + "> {");
            sb.Append(turtle);
            sb.Append("} }");

            GetRecordChunks(turtle);
            Console.WriteLine("Len: " + turtle.Length);

            return await Query(sb.ToString());
        }
        
        private List<string> GetRecordChunks(string query)
        {
            List<string> chunks = new();
            Regex detectRDFRecordEnding = new(@"<.*?>\s+\.");
            string[] lines = query.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int start = 0;
            foreach (Match match in detectRDFRecordEnding.Matches(query))
            {
                chunks.Append(query.Substring(start, match.Index + match.Length));
                start = match.Index + 1;
            }

            Console.WriteLine("LenC:" + chunks.Sum(x => x.Length));
            Console.WriteLine($"Chunks: {chunks.Count}");
            return chunks;
        }
    }
}