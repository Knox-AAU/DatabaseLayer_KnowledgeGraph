using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RDFApi.Controllers
{
    public sealed class QueryController : Controller
    {
        [HttpGet, Route("/[controller]/")]
        public async Task<IActionResult> Query(string query)
        {
            string? virtuosoEndpoint = Environment.GetEnvironmentVariable("VIRTUOSO_ENDPOINT");
            if (virtuosoEndpoint == null) 
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "VIRTUOSO_ENDPOINT environment variable not set");
            }

            try
            {
                string queryResult = await new VirtuosoDataStore(virtuosoEndpoint).Query(query);
                
                return Ok(queryResult); 
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, e.Message);
            }
        }
        
        [HttpPost, Route("/[controller]/")]
        public async Task<IActionResult> Insert(IFormFile turtleFile)
        {
            string? virtuosoEndpoint = Environment.GetEnvironmentVariable("VIRTUOSO_ENDPOINT");
            if (virtuosoEndpoint == null) 
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "VIRTUOSO_ENDPOINT environment variable not set");
            }

            try
            {
                // Read the file
                await using (var stream = new System.IO.MemoryStream())
                {
                    await turtleFile.CopyToAsync(stream);
                    string turtle = System.Text.Encoding.UTF8.GetString(stream.ToArray());

                    string insertResponse = await new VirtuosoDataStore(virtuosoEndpoint).InsertTurtleGraph(turtle);

                    return Ok(insertResponse);
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, e.Message);
            }
        }
    }
}