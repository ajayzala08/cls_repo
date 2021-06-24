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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clscorporatesubscriberController : ApiController
    {
        public HttpResponseMessage Postclscorporatesubscriber(List<corporatesubscribermodel> model)
        {
            HttpResponseMessage response;
            try
            {
                int total = 0, success = 0, fail = 0;
                using (var db = new CompanyFormationdbEntities())
                {
                    if (model.Count > 0)
                    {
                        decimal dcfid = model[0].cfid;
                        var find = db.cls_corporatesubscriber_tbl.Where(x => x.cfid == dcfid).ToList();
                        if (find.Count > 0)
                        {
                            deletecorporatesubscriber(dcfid);
                        }
                        bool checkcompanyname = true; //checkcorporatesubscriber(model);
                        if (checkcompanyname)
                        {
                            bool checknum = true; //checknumber(model);
                            if (checknum)
                            {
                                bool checkcomdir = true; //checkdirectorname(model);
                                if (checkcomdir)
                                {
                                    for (int i = 0; i < model.Count; i++)
                                    {
                                        if ((model[i].companyname != null) && (model[i].companyname.ToString() != ""))
                                        {
                                            total++;
                                            cls_corporatesubscriber_tbl tbl = new cls_corporatesubscriber_tbl();
                                            tbl.cfid = model[i].cfid;
                                            tbl.companyname = model[i].companyname;
                                            tbl.companyphonenumber = model[i].companyphonenumber;
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
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company director name already exists");
                                }
                            }
                            else
                            {
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Phone number already exists");
                            }
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Comapny name already exists");
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

        private bool checkcorporatesubscriber(List<corporatesubscribermodel> model)
        {

            bool checkcorporatesubscribername = false;

            for (int i = 0; i < model.Count; i++)
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    if ((model[i].companyname != null) && (model[i].companyname.ToString() != ""))
                    {
                        var Companyname = model[i].companyname.ToLower();
                        var find = db.cls_corporatesubscriber_tbl.Where(x => x.companyname.ToLower() == Companyname).FirstOrDefault();
                        //var find = db.cls_director_tbl.Where(x => x.name.ToLower() == model[i].name.ToLower()).ToList();


                        if (find != null)
                        {
                            checkcorporatesubscribername = false;
                            goto corporatesubscribercondition;
                        }
                        else
                        {
                            checkcorporatesubscribername = true;
                        }
                    }
                    else
                    {
                        checkcorporatesubscribername = true;
                    }
                }
            }
        corporatesubscribercondition:
            return checkcorporatesubscribername;
        }

        private bool checknumber(List<corporatesubscribermodel> model)
        {

            bool checknumber = false;

            for (int i = 0; i < model.Count; i++)
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    if (model[i].companyphonenumber != null && model[i].companyphonenumber != "")
                    {
                        var pnumber = model[i].companyphonenumber.Trim();
                        var find = db.cls_corporatesubscriber_tbl.Where(x => x.companyphonenumber.Trim() == pnumber).FirstOrDefault();
                        //var find = db.cls_director_tbl.Where(x => x.name.ToLower() == model[i].name.ToLower()).ToList();


                        if (find != null)
                        {
                            checknumber = false;
                            goto corporatesubscribercondition;
                        }
                        else
                        {
                            checknumber = true;
                        }
                    }
                    else
                    {
                        checknumber = true;
                    }
                }
            }
        corporatesubscribercondition:
            return checknumber;
        }

        private bool checkdirectorname(List<corporatesubscribermodel> model)
        {

            bool checkdirector = false;

            for (int i = 0; i < model.Count; i++)
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    if ((model[i].companydirector != null) && (model[i].companydirector.ToString() != ""))
                    {
                        var directorname = model[i].companydirector.ToLower();
                        var find = db.cls_corporatesubscriber_tbl.Where(x => x.companydirector.ToLower() == directorname).FirstOrDefault();
                        //var find = db.cls_director_tbl.Where(x => x.name.ToLower() == model[i].name.ToLower()).ToList();


                        if (find != null)
                        {
                            checkdirector = false;
                            goto corporatesubscribercondition;
                        }
                        else
                        {
                            checkdirector = true;
                        }
                    }
                    else
                    {
                        checkdirector = true;
                    }
                }
            }
        corporatesubscribercondition:
            return checkdirector;
        }

        protected void deletecorporatesubscriber(decimal cfid)
        {
            using (var db = new CompanyFormationdbEntities())
            {

                db.cls_corporatesubscriber_tbl.RemoveRange(db.cls_corporatesubscriber_tbl.Where(x => x.cfid == cfid));
                db.SaveChanges();

            }
        }
    }
}
