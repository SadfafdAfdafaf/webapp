using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace authorization.Controllers
{
    [RoutePrefix("test")]
    public class checkController : ApiController
    {
        [Authorize]
        [Route("token")]
        public IHttpActionResult Get()
        {

            return Ok();
        }
    }
}
