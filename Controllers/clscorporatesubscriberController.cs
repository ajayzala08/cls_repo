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
    public class clscorporatesubscriberController : ApiController
    {
        public HttpResponseMessage Postclscorporatesubscriber(List<corporatesubscribermodel> model)
        {
            HttpResponseMessage response;
            try
            {
                int total = 0, success = 0, fail = 0;
                using (var db = new CompanyFormation_dbEntities())
                {
                    if (model.Count > 0)
                    {
                        for (int i = 0; i < model.Count; i++)
                        {
                            total++;
                            cls_corporatesubscriber_tbl tbl = new cls_corporatesubscriber_tbl();
                            tbl.cfid = model[i].cfid;
                            tbl.companyname = model[i].companyname;
                            tbl.companydirector = model[i].companydirector;
                            tbl.registerofficeaddress = model[i].registeroffice;
                            tbl.addressline2 = model[i].addressline2;
                            tbl.addressline3 = model[i].addressline3;
                            tbl.postalcode = model[i].postalcode;
                            tbl.country = model[i].country;
                            tbl.numberofshare = model[i].numberofshare;
                            db.cls_corporatesubscriber_tbl.Add(tbl);
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
