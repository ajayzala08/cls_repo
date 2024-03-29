﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsbenefeficialownerController : ApiController
    {
        public HttpResponseMessage postclsbeneficialowner(List<beneficialownermodel> model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    if (model.Count > 0)
                    {
                        decimal dcfid = model[0].cfid;
                        var find = db.cls_beneficialowner_tbl.Where(x => x.cfid == dcfid).ToList();
                        if (find.Count > 0)
                        {
                            deletebo(dcfid);
                        }
                        bool checkbo = true; //checkbeneficalownername(model);
                        if (checkbo)
                        {
                            int total = 0, success = 0, fail = 0;
                            for (int i = 0; i < model.Count; i++)
                            {
                                
                                if (model[i].cfid > 0 && model[i].cfid.ToString() != "")
                                {
                                    if ((model[i].firstname != null) && (model[i].firstname.ToString() != "") && (model[i].lastname != null) && (model[i].lastname.ToString() != ""))
                                    {
                                        total++;
                                        string fullname = model[i].firstname.ToString().Trim() + " " + model[i].lastname.ToString().Trim();
                                        cls_beneficialowner_tbl tbl = new cls_beneficialowner_tbl();
                                        tbl.cfid = model[i].cfid;
                                        tbl.name = fullname;
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
                            }
                            response = Request.CreateResponse(HttpStatusCode.Created, success.ToString() + "/" + total.ToString() + " Success");
                            return response;
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Name already exists.");
                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
                       
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }


        }
        private bool checkbeneficalownername(List<beneficialownermodel> model)
        {

            bool checkboname = false;

            for (int i = 0; i < model.Count; i++)
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    if ((model[i].firstname != null) && (model[i].firstname.ToString() != "") && (model[i].lastname != null) && (model[i].lastname.ToString() != ""))
                    {
                        string fullname = model[i].firstname.ToString() + " " + model[i].lastname.ToString();
                        var name = fullname.ToLower();
                        var find = db.cls_beneficialowner_tbl.Where(x => x.name.ToLower() == name).FirstOrDefault();
                        //var find = db.cls_director_tbl.Where(x => x.name.ToLower() == model[i].name.ToLower()).ToList();


                        if (find != null)
                        {
                            checkboname = false;
                            goto corporatesubscribercondition;
                        }
                        else
                        {
                            checkboname = true;
                        }
                    }
                    else
                    {
                        checkboname = true;
                    }
                }
            }
        corporatesubscribercondition:
            return checkboname;
        }

        protected void deletebo(decimal cfid)
        {
            using (var db = new CompanyFormationdbEntities())
            {

                db.cls_beneficialowner_tbl.RemoveRange(db.cls_beneficialowner_tbl.Where(x => x.cfid == cfid));
                db.SaveChanges();

            }
        }
    }
}
