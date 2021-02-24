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
    public class clsaddressdetailsController : ApiController
    {
        public HttpResponseMessage postclsaddressdetails(addressdetailsmodel model)
        {
            HttpResponseMessage respose;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    if (model.cfid > 0 && model.cfid.ToString() != "")
                    {
                        cls_addressdetails_tbl tbl = new cls_addressdetails_tbl();
                        tbl.cfid = model.cfid;
                        tbl.roaddressline1 = model.roaddressline1;
                        tbl.roaddressline2 = model.roaddressline2;
                        tbl.oraddressline3 = model.roaddressline3;
                        tbl.ropostalcode = model.ropostalcode;
                        tbl.caaddressline1 = model.caaddressline1;
                        tbl.caaddressline2 = model.caaddressline2;
                        tbl.caaddressline3 = model.caaddressline3;
                        tbl.capostalcode = model.capostalcode;
                        tbl.roisalsothebusinessorcaaddress = model.roisalsothebusinessorcaaddress;
                        db.cls_addressdetails_tbl.Add(tbl);
                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            respose = Request.CreateResponse(HttpStatusCode.Created, "Success");

                        }
                        else
                        {
                            respose = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Fail");

                        }
                        return respose;
                    }
                    else
                    {
                        respose = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Fail");
                        return respose;
                    }
                }
                
            }
            catch (Exception ex)
            {
                respose = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return respose;
            }

        }
    }
}
