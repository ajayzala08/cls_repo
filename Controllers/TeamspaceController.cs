using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using WebApplication3.Models;
using Newtonsoft.Json;
using System.Text;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TeamspaceController : ApiController
    {
        string baseurl = "https://clscharteredsecretaries.eu.teamwork.com/";
        public async Task<HttpResponseMessage> get()
        {
            HttpResponseMessage response = new HttpResponseMessage();


            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseurl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "tkn.v1_NTBmYWYwMzQtYTU5OC00OGFlLTk4ZjEtYTBjYjVkZTc5YWI2LTY4MzY1OC41ODA3MDMuRVU=");
                
                var res = await client.GetAsync("spaces/api/v1/spaces.json");
                return res;
            }
            
        }

        public async Task<HttpResponseMessage> post(teamspacemodel model)
        
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseurl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "tkn.v1_NTBmYWYwMzQtYTU5OC00OGFlLTk4ZjEtYTBjYjVkZTc5YWI2LTY4MzY1OC41ODA3MDMuRVU=");
                

                var myjson = new
                {
                    space = model
                    
                };
                
                
                var example = JsonConvert.SerializeObject(myjson);
                
                var httpcontent = new StringContent(example, Encoding.UTF8, "application/json");

                var res = await client.PostAsync("spaces/api/v1/spaces.json",  httpcontent);

                return res;
            }
        }
       
    }
}
