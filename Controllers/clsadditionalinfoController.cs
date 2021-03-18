using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsadditionalinfoController : ApiController
    {
        public HttpResponseMessage postclsadditionalinfo(additinoalinfomodel model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    if (model.cfid > 0)
                    {
                        cls_additionalinfo_tbl tbl = new cls_additionalinfo_tbl();
                        tbl.cfid = model.cfid;
                        tbl.additionalinformation = model.addtionalinfo;
                        db.cls_additionalinfo_tbl.Add(tbl);
                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.Created, "Success");
                        }
                        else
                        {
                            response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Fail");
                        }
                    }
                    else
                    {
                        response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Fail");
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }
        }
    }
}
