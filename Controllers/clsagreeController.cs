using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsagreeController : ApiController
    {
        public HttpResponseMessage post(agreemodel agreemodel)
        {
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    Dictionary<string, decimal> keys = new Dictionary<string, decimal>();
                    cls_agree_tbl tbl = new cls_agree_tbl();
                    tbl.agree = agreemodel.agree;
                    tbl.companypacktype = agreemodel.companypacktype;
                    tbl.incorporationtype = agreemodel.incorporationtype;
                    tbl.nonthirdparties = agreemodel.nonthirdparties;
                    tbl.paymenttype = agreemodel.paymenttype;
                    tbl.name = agreemodel.name;
                    tbl.companyname = agreemodel.companyname;
                    tbl.addressline1 = agreemodel.addressline1;
                    tbl.addressline2 = agreemodel.addressline2;
                    tbl.addressline3 = agreemodel.addressline3;
                    tbl.postcode = agreemodel.postal;
                    tbl.phonenumber = agreemodel.phone;
                    tbl.email = agreemodel.email;

                    db.cls_agree_tbl.Add(tbl);
                    db.SaveChanges();
                    db.Entry(tbl).GetDatabaseValues();
                    var cfid = tbl.cfid;
                    keys.Add("cfid", cfid);
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, keys);
                    return response;
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }
    }
}
