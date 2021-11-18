using System;
using Microsoft.AspNetCore.Mvc;
using VDS.RDF.Storage;

namespace RDFApi.Controllers
{
    public sealed class QueryController : Controller
    {
        [HttpGet, Route("/[controller]/status")]
        public IActionResult Status()
        {
            var vm = new VirtuosoManager(
                Environment.GetEnvironmentVariable("DBA_HOST"), 
                int.Parse(Environment.GetEnvironmentVariable("DBA_PORT")), 
                "DB", 
                Environment.GetEnvironmentVariable("DBA_USERNAME"), 
                Environment.GetEnvironmentVariable("DBA_PASSWORD"));
            return Ok(vm.IsReady);
        }
    }
}