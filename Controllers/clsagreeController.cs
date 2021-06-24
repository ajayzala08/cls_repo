using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;
using System.Linq;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsagreeController : ApiController
    {
        //create
        public HttpResponseMessage post(agreemodel agreemodel)
        {
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    HttpResponseMessage response = new HttpResponseMessage();



                    //var checkeckname = db.cls_agree_tbl.Where(x => x.name.ToLower() == agreemodel.name.ToLower()).FirstOrDefault();
                    //if (checkeckname == null)
                    //{
                    //    var checkcompanyname = db.cls_agree_tbl.Where(x => x.companyname.ToLower() == agreemodel.companyname.ToLower()).FirstOrDefault();
                    //    if (checkcompanyname == null)
                    //    {
                    //        var checkemail = db.cls_agree_tbl.Where(x => x.email.ToLower() == agreemodel.email.ToLower()).FirstOrDefault();
                    //        if (checkemail == null)
                    //        {

                    //            var checkphone = db.cls_agree_tbl.Where(x => x.phonenumber == agreemodel.phone).FirstOrDefault();
                    //            if (checkphone == null)
                    //            {
                    #region clsagree
                    cls_agree_tbl tbl;
                    tbl = db.cls_agree_tbl.Where(x => x.cfid == agreemodel.cfid).FirstOrDefault();
                    if (tbl != null)
                    {
                        tbl.agree = agreemodel.agree;
                        tbl.companypacktype = agreemodel.companypacktype;
                        tbl.incorporationtype = agreemodel.incorporationtype;
                        tbl.nonthirdparties = agreemodel.nonthirdparties;
                        tbl.paymenttype = agreemodel.paymenttype;
                        tbl.name = agreemodel.name;
                        tbl.companyname = agreemodel.companyname.Trim();
                        tbl.addressline1 = agreemodel.addressline1;
                        tbl.addressline2 = agreemodel.addressline2;
                        tbl.addressline3 = agreemodel.addressline3;
                        tbl.postcode = agreemodel.postal;
                        tbl.phonenumber = agreemodel.phone;
                        tbl.email = agreemodel.email;
                        tbl.username = agreemodel.username;

                        db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        response = Request.CreateResponse(HttpStatusCode.Created, "Success");
                    }
                    else
                    {

                        Dictionary<string, decimal> keys = new Dictionary<string, decimal>();
                        tbl = new cls_agree_tbl();

                        tbl.agree = agreemodel.agree;
                        tbl.companypacktype = agreemodel.companypacktype;
                        tbl.incorporationtype = agreemodel.incorporationtype;
                        tbl.nonthirdparties = agreemodel.nonthirdparties;
                        tbl.paymenttype = agreemodel.paymenttype;
                        tbl.name = agreemodel.name;
                        tbl.companyname = agreemodel.companyname.Trim();
                        tbl.addressline1 = agreemodel.addressline1;
                        tbl.addressline2 = agreemodel.addressline2;
                        tbl.addressline3 = agreemodel.addressline3;
                        tbl.postcode = agreemodel.postal;
                        tbl.phonenumber = agreemodel.phone;
                        tbl.email = agreemodel.email;
                        tbl.username = agreemodel.username;

                        db.cls_agree_tbl.Add(tbl);
                        db.SaveChanges();
                        db.Entry(tbl).GetDatabaseValues();
                        var cfid = tbl.cfid;
                        keys.Add("cfid", cfid);
                        response = Request.CreateResponse(HttpStatusCode.Created, keys);
                    }
                                    #endregion clsagree
                    //            }
                    //            else
                    //            {
                    //                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Phone Number '" + agreemodel.phone + "' already exists.");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Email '" + agreemodel.email + "' already exists.");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Name '" + agreemodel.companyname + "' already exists.");
                    //    }
                    //}
                    //else
                    //{
                    //    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Name '" + agreemodel.name + "' already exists.");
                        
                    //}
                    return response; 
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }


        //Edit 
        public HttpResponseMessage put(agreemodel agreemodel)
        {
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    //var checkeckname = db.cls_agree_tbl.Where(x => x.name.ToLower() == agreemodel.name.ToLower()).FirstOrDefault();
                    //if (checkeckname == null)
                    //{
                    //    var checkcompanyname = db.cls_agree_tbl.Where(x => x.companyname.ToLower() == agreemodel.companyname.ToLower()).FirstOrDefault();
                    //    if (checkcompanyname == null)
                    //    {
                    //        var checkemail = db.cls_agree_tbl.Where(x => x.email.ToLower() == agreemodel.email.ToLower()).FirstOrDefault();
                    //        if (checkemail == null)
                    //        {

                    //            var checkphone = db.cls_agree_tbl.Where(x => x.phonenumber == agreemodel.phone).FirstOrDefault();
                    //            if (checkphone == null)
                    //            {
                    #region clsagree

                   // Dictionary<string, decimal> keys = new Dictionary<string, decimal>();
                    cls_agree_tbl tbl = db.cls_agree_tbl.Where(x => x.cfid == agreemodel.cfid).FirstOrDefault();
                    if (tbl != null)
                    {
                        tbl.agree = agreemodel.agree;
                        tbl.companypacktype = agreemodel.companypacktype;
                        tbl.incorporationtype = agreemodel.incorporationtype;
                        tbl.nonthirdparties = agreemodel.nonthirdparties;
                        tbl.paymenttype = agreemodel.paymenttype;
                        tbl.name = agreemodel.name;
                        tbl.companyname = agreemodel.companyname.Trim();
                        tbl.addressline1 = agreemodel.addressline1;
                        tbl.addressline2 = agreemodel.addressline2;
                        tbl.addressline3 = agreemodel.addressline3;
                        tbl.postcode = agreemodel.postal;
                        tbl.phonenumber = agreemodel.phone;
                        tbl.email = agreemodel.email;
                        tbl.username = agreemodel.username;

                        db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        response = Request.CreateResponse(HttpStatusCode.Created, "Updated");
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.Created, "Fail");
                    }
                    //    db.Entry(tbl).GetDatabaseValues();
                    //var cfid = tbl.cfid;
                    //keys.Add("cfid", cfid);
                    
                    #endregion clsagree
                    //            }
                    //            else
                    //            {
                    //                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Phone Number '" + agreemodel.phone + "' already exists.");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Email '" + agreemodel.email + "' already exists.");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Name '" + agreemodel.companyname + "' already exists.");
                    //    }
                    //}
                    //else
                    //{
                    //    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Name '" + agreemodel.name + "' already exists.");

                    //}
                    return response;
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }

        protected void deleteclsagree(decimal cfid)
        {
            using (var db = new CompanyFormationdbEntities())
            {

                db.cls_agree_tbl.RemoveRange(db.cls_agree_tbl.Where(x => x.cfid == cfid));
                db.SaveChanges();

            }
        }
    }
}
