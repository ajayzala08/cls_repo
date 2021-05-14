﻿using System;
using System.Linq;
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
                using (var db = new CompanyFormationdbEntities())
                {
                    var checkname = db.cls_secretary_tbl.Where(x => x.name.ToLower() == model.name.ToLower()).FirstOrDefault();
                    if (checkname == null)
                    {
                        var checkcompanyname = db.cls_secretary_tbl.Where(x => x.companyname.ToLower() == model.companyname.ToLower() && model.companyname.ToString() != "").FirstOrDefault();
                        if (checkcompanyname == null)
                        {
                            var checkcompanyno = db.cls_secretary_tbl.Where(x => x.companynumber == model.companynumber && model.companynumber != 0).FirstOrDefault();
                            if (checkcompanyno == null)
                            {
                                
                                var checkcompanydirector = db.cls_secretary_tbl.Where(x => x.companydirector.ToLower() == model.companydirector.ToLower() && model.companydirector.ToString() !="").FirstOrDefault();
                                if (checkcompanydirector == null)
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
                                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
                                    }
                                }
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Director '" + model.companydirector + "' already exists.");
                                }
                            }
                            else {
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Number '" + model.companynumber + "' already exists.");
                            }
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Name '" + model.companyname + "' already exists.");
                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Name '" + model.name + "' already exists.");
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
