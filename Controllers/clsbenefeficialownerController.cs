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
    public class clsbenefeficialownerController : ApiController
    {
        public HttpResponseMessage postclsbeneficialowner(List<beneficialownermodel> model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    if (model.Count > 0)
                    {
                        int total = 0, success = 0, fail = 0;
                        for (int i = 0; i < model.Count; i++)
                        {
                            total++;
                            if (model[i].cfid > 0 && model[i].cfid.ToString() != "")
                            {
                                cls_beneficialowner_tbl tbl = new cls_beneficialowner_tbl();
                                tbl.cfid = model[i].cfid;
                                tbl.name = model[i].name;
                                tbl.addressline1 = model[i].addressline1;
                                tbl.addressline2 = model[i].addressline2;
                                tbl.addressline3 = model[i].addressline3;
                                tbl.postalcode = model[i].postalcode;
                                tbl.country = model[i].country;
                                tbl.nationality = model[i].nationality;
                                tbl.occupation = model[i].occupation;
                                tbl.natureofownership = model[i].natureofownership;
                                db.cls_beneficialowner_tbl.Add(tbl);
                                var result = db.SaveChanges();
                                if (result > 0)
                                {
                                    success++;
                                }
                                else
                                {
                                    fail++;
                                }
                            }
                        }
                        response = Request.CreateResponse(HttpStatusCode.Created, success.ToString() + "/" + total.ToString() + " Success");
                        return response;
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
                        return response;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }

            
        }
    }
}
