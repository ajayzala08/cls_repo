using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class clscompanyincorporationController : ApiController
    {
        public HttpResponseMessage postclscompanyincorporation(companyincorporationdetailsmodel model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    cls_companyincorporation_tbl tbl = new cls_companyincorporation_tbl();
                    tbl.cfid = model.cfid;
                    tbl.firstchoice = model.firstchoice;
                    tbl.secondchoice = model.secondchoice;
                    tbl.thirdchoice = model.thirdchoice;
                    tbl.principalactivity = model.principalactivity;
                    tbl.additionalwording = model.additionwording;
                    tbl.companytype = model.companytype;
                    db.cls_companyincorporation_tbl.Add(tbl);
                    var result = db.SaveChanges();
                    if (result > 0)
                    {
                        response = Request.CreateResponse(HttpStatusCode.Created, "Success");
                        
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }
        }
    }
}
