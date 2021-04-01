using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading;
using System.Threading.Tasks;
using WebApplication3.Models;
using Newtonsoft.Json;
using System.Text;

namespace WebApplication3.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class DealController : ApiController
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

                var res = await client.GetAsync("crm/api/v2/deals.json");
                return res;
            }
        }

        public async Task<HttpResponseMessage> Post(dealmainmodel model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseurl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "tkn.v1_YmEzYzg4MzctYjE2OC00NDlmLTk0YjYtZjlmYzdmYjMxNWYxLTY4MzY1OC41ODA3MDMuRVU=");
                currencycls ccls = new currencycls();
                ccls.id = model.currencyId;
                ccls.type = model.currencyType;

                stagecls scls = new stagecls();
                scls.id = model.stageId;
                scls.type = model.stageType;

                customcls cucls = new customcls();

                List<string> objproduct = new List<string>();
                if (model.products.Count > 0)
                {
                    foreach (var item in model.products)
                    {
                        objproduct.Add(item);
                    }
                }

                List<string> objcontacts = new List<string>();
                if (model.contacts.Count>0)
                {
                    foreach (var item in model.contacts)
                    {
                        objcontacts.Add(item);
                    }
                }

                dealmodel dmodel = new dealmodel();
                dmodel.title = "Test Arche AZ";
                dmodel.state = "open";
                dmodel.customValue = "custvalue";
                dmodel.currency = ccls;
                dmodel.company = null;
                dmodel.expectedCloseDate = System.DateTime.Now.AddDays(10);
                dmodel.stage = scls;
                dmodel.products = objproduct;
                dmodel.contacts = objcontacts;
                dmodel.custom = cucls;
                
                
                
                
                var myjson = new
                {
                    deal = dmodel

                };


                var example = JsonConvert.SerializeObject(myjson);

                var httpcontent = new StringContent(example, Encoding.UTF8, "application/json");

                var res = await client.PostAsync("crm/api/v2/deals.json",httpcontent);
                return res;
            }

        }
    }
}
