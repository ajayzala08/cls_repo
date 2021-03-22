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
    public class clscfidController : ApiController
    {
        public HttpResponseMessage get()
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    var cfid = db.cls_agree_tbl.OrderByDescending(x => x.cfid).FirstOrDefault().cfid.ToString();
                    if (cfid != null && cfid.ToString() != "")
                    {
                        Dictionary<string, string> keyValues = new Dictionary<string, string>();
                        keyValues.Add("cfid", cfid);
                        response = Request.CreateResponse(HttpStatusCode.OK, keyValues);
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }
        }
    }
}
