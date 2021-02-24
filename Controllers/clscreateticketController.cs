using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebApplication3.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class clscreateticketController : ApiController
    {
        public HttpResponseMessage postclscreateticket()
        {
            HttpResponseMessage response= new HttpResponseMessage();

            return response;
            
        }
    }
}
