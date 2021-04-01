using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebApplication3.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class StageController : ApiController
    {
        string baseurl = "https://clscharteredsecretaries.eu.teamwork.com/";
        public async Task<HttpResponseMessage> get()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseurl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "tkn.v1_YmEzYzg4MzctYjE2OC00NDlmLTk0YjYtZjlmYzdmYjMxNWYxLTY4MzY1OC41ODA3MDMuRVU=");

                var res = await client.GetAsync("crm/api/v1/deals.json");
                return res;
            }
        }
    }
}
