using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clssecretaryController : ApiController
    {
        public HttpResponseMessage postclssecretary(secretarymodel model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    cls_secretary_tbl tbl = new cls_secretary_tbl();
                    tbl.cfid = model.cfid;
                    tbl.name = model.name;
                    if (model.dob != null && model.dob.Year > 1)
                    {
                        tbl.dob = Convert.ToDateTime(model.dob);
                    }
                    tbl.addressline1 = model.addressline1;
                    tbl.addressline2 = model.addressline2;
                    tbl.addressline3 = model.addressline3;
                    tbl.postalcode = model.postal;
                    tbl.country = model.country;
                    tbl.companyname = model.companyname;
                    tbl.companynumber = model.companynumber;
                    tbl.companydirector = model.companydirector;
                    tbl.companyregionaloffice = model.companyregiseroffice;
                    tbl.companyaddressline1 = model.companyaddressline1;
                    tbl.companyaddressline2 = model.companyaddressline2;
                    tbl.compnaypostal = model.companypostal;
                    tbl.compnaycountry = model.companycountry;
                    db.cls_secretary_tbl.Add(tbl);
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
