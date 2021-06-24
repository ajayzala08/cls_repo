using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clssharecapitalController : ApiController
    {
        //Create
        public HttpResponseMessage Postclssharecapital(sharecapitalmodel model)
        {
            HttpResponseMessage response =new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    cls_sharecapital_tbl tbl;

                    tbl = db.cls_sharecapital_tbl.Where(x => x.cfid == model.cfid).FirstOrDefault();
                    if (tbl != null)
                    {
                        tbl.cfid = model.cfid;
                        tbl.issuedsharecapital = model.issuedsharecapital;
                        tbl.nominalamoutpershare = model.nominalamountpershare;
                        tbl.shareclass = model.shareclass;
                        tbl.authorisedsharecapital = model.authorisedsharecapital;
                        db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
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
                        tbl = new cls_sharecapital_tbl();
                        tbl.cfid = model.cfid;
                        tbl.issuedsharecapital = model.issuedsharecapital;
                        tbl.nominalamoutpershare = model.nominalamountpershare;
                        tbl.shareclass = model.shareclass;
                        tbl.authorisedsharecapital = model.authorisedsharecapital;
                        db.cls_sharecapital_tbl.Add(tbl);
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
                }
                return response;
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }
        }
        //Eidt
        public HttpResponseMessage put(sharecapitalmodel model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    cls_sharecapital_tbl tbl = db.cls_sharecapital_tbl.Where(x => x.cfid == model.cfid).FirstOrDefault();
                    if (tbl != null)
                    {
                        tbl.cfid = model.cfid;
                        tbl.issuedsharecapital = model.issuedsharecapital;
                        tbl.nominalamoutpershare = model.nominalamountpershare;
                        tbl.shareclass = model.shareclass;
                        tbl.authorisedsharecapital = model.authorisedsharecapital;
                        db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.Created, "Updated");
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
