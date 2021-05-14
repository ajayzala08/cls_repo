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
    public class clsdirectorController : ApiController
    {
        public HttpResponseMessage postclsdirector(List<directormodel> model)
        {
            HttpResponseMessage response;
            try
            {
                int success = 0, fail = 0, total = 0;
                using (var db = new CompanyFormationdbEntities())
                {
                    if (model.Count > 0)
                    {
                        bool checkdrirectroname = checkdirectors(model);
                        if (checkdrirectroname)
                        {
                            for (int i = 0; i < model.Count; i++)
                            {
                               
                                if (model[i].cfid > 0)
                                {
                                    if ((model[i].name != null) && (model[i].name.ToString() != ""))
                                    {
                                        total++;

                                        cls_director_tbl tbl = new cls_director_tbl();
                                        tbl.cfid = model[i].cfid;
                                        tbl.name = model[i].name;
                                        if (model[i].dob != null && model[i].dob.Year > 1)
                                        {
                                            tbl.dob = model[i].dob;
                                        }
                                        tbl.occupation = model[i].occupation;
                                        tbl.addressline1 = model[i].addressline1;
                                        tbl.addressline2 = model[i].addressline2;
                                        tbl.addressline3 = model[i].addressline3;
                                        tbl.postalcode = model[i].postal;
                                        tbl.country = model[i].country;
                                        tbl.nationality = model[i].nationality;
                                        tbl.otherdirectorship1 = model[i].otherdirectorship1;
                                        tbl.otherdirectorship2 = model[i].otherdirectorship2;
                                        tbl.otherdirectorship3 = model[i].otherdirectorship3;
                                        tbl.restricted = model[i].restricted;
                                        tbl.numberofshare = model[i].numberofshare;
                                        tbl.beneficialowner = model[i].beneficialowner;
                                        db.cls_director_tbl.Add(tbl);
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
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Director name already exists");
                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
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
        private bool checkdirectors(List<directormodel> model)
        {

            bool checkdrirectroname = false;

            for (int i = 0; i < model.Count; i++)
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    if ((model[i].name != null) && (model[i].name.ToString() != ""))
                    {
                        var name = model[i].name.ToLower();
                        var find = (from c in db.cls_director_tbl where c.name.ToLower() == name select c).FirstOrDefault();
                        if (find != null)
                        {
                            checkdrirectroname = false;
                            goto directornamecheckcondition;
                        }
                        else
                        {
                            checkdrirectroname = true;
                        }
                    }
                    else
                    {
                        checkdrirectroname = true;
                    }
                }
            }
        directornamecheckcondition:
            return checkdrirectroname;
        }
    }
}
