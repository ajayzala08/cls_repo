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
    public class clssubscriberController : ApiController
    {
        public HttpResponseMessage postsubscriber(List<subscribermodel> model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    int total = 0, success = 0, fail = 0;
                    if (model.Count > 0)
                    {
                        for (int i = 0; i < model.Count; i++)
                        {
                            total++;
                            if (model[i].cfid > 0)
                            {
                                cls_subscriber_tbl tbl = new cls_subscriber_tbl();
                                tbl.cfid = model[i].cfid;
                                tbl.name = model[i].name;
                                tbl.addressline1 = model[i].addressline1;
                                tbl.addressline2 = model[i].addressline2;
                                tbl.addressline3 = model[i].addressline3;
                                tbl.postalcode = model[i].postalcode;
                                tbl.country = model[i].country;
                                tbl.nationality = model[i].nationality;
                                tbl.occupation = model[i].occupation;
                                tbl.numberofshare = model[i].numberofshare;
                                tbl.beneficialowner = model[i].beneficialowner;
                                db.cls_subscriber_tbl.Add(tbl);
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
