using System;
using System.Net;
using System.Threading.Tasks;
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
    }
}