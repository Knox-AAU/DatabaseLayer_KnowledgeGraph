﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
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

        public async Task<string> InsertTurtleGraph(string turtle)
        {
            IGraph g = new Graph()
            {
                BaseUri = uri,
            };
            
            GraphHandler graphHandler = new(g);
            TurtleParser parser = new();
            parser.Load(graphHandler, new StringReader(turtle));
            
            Console.WriteLine($"Triples in graph: {g.Triples.Count}");

            NTriplesFormatter formatter = new();
            StringBuilder sb = new();
            
            sb.Append("INSERT DATA { GRAPH <pls> {");
            sb.Append(turtle);
            sb.Append("} }");

            return await Query(sb.ToString());
        }
    }
}