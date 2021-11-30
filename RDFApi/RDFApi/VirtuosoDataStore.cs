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

            FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("query", query)
            });
            
            HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.PostAsync(uriBuilder.ToString(), formContent);

            if (!response.IsSuccessStatusCode)
            {
                // Virtuoso sends errors in SPARQL through content, which we want to send to the users.
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"{response.StatusCode}: {error}");
            }
            
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> InsertTurtleGraph(string turtle, string? graphName = "knox")
        {
            IGraph g = new Graph()
            {
                BaseUri = uri,
            };
            
            GraphHandler graphHandler = new(g);
            TurtleParser parser = new();
            parser.Load(graphHandler, new StringReader(turtle));
            
            Console.WriteLine($"Triples in given graph: {g.Triples.Count}");

            string[] chunkedQueries = ChunkRecords(turtle, "\n\n", 50);

            foreach (string chunkedQuery in chunkedQueries)
            {
                StringBuilder sb = new();
                
                sb.Append("INSERT DATA { GRAPH <" + graphName + "> {");
                sb.Append(chunkedQuery);
                sb.Append("} }");
 
                await Query(sb.ToString());
            }

            return "OK";
        }

        private string[] ChunkRecords(string input, string separator, int chunkSize = 10)
        {
            List<string> chunks = new();
            string[] strings = input.Split(separator);
            
            for (int i = 0; i < strings.Length / chunkSize; i++)
            {
                string[] chunk = strings.Skip(i * chunkSize).Take(chunkSize).ToArray();
                chunks.Add(string.Join(separator, chunk));
            }

            int remainder = strings.Length % chunkSize;
            int taken = chunks.Count * chunkSize;

            string[] lastChunk = strings.Skip(taken).Take(remainder).ToArray();
            chunks.Add(string.Join(separator, lastChunk));
            
            return chunks.ToArray();
        }
    }
}