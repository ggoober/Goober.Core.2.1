using Hermes.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace Goober.WebApi.Controllers
{
    public class PingApiController : Controller
    {
        private readonly IConfiguration _configuration;

        public PingApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/ping/get")]
        public PingResponse Get()
        {
            var ret = new PingResponse { IsSuccess = true };

            var watch = new Stopwatch();
            watch.Start();

            var val = _configuration["test-emtpy"];

            watch.Stop();

            ret.TimeTakenMilliseconds = watch.ElapsedMilliseconds;

            return ret;
        }

        [HttpGet]
        [Route("api/ping/date")]
        public DateTime GetDate()
        {
            return DateTime.Now;
        }
    }

}
