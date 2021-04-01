using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class clsregistrationController : ApiController
    {
        public EntityState EntityState { get; private set; }

        public HttpResponseMessage post(registrationmodel registration)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormation_dbEntities()) 
                {
                    var finduser = db.cls_usermst_tbl.Where(x => x.user_fullname.ToLower() == registration.fullname.ToLower() || x.user_code==registration.empcode).FirstOrDefault();
                    if (finduser != null)
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fullname/Employee Code Already Exists.");
                    }
                    else
                    {
                       
                            cls_usermst_tbl tbl = new cls_usermst_tbl();
                            tbl.user_code = registration.empcode;
                            tbl.user_fullname = registration.fullname;
                            tbl.user_emailid = registration.email;
                            tbl.user_username = registration.username;
                            tbl.user_pwd = registration.password;
                            tbl.user_department = registration.department;
                            tbl.user_phone = registration.phone;
                            tbl.user_role = registration.role;
                            tbl.user_active = 0;
                            tbl.user_delete = 1;
                            tbl.user_createdon = System.DateTime.Now;
                            db.cls_usermst_tbl.Add(tbl);
                            var result = db.SaveChanges();
                            if (result > 0)
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK, "User created successfully");
                            }
                            else
                            {
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail to create user");
                            }
                       
                    }
                    return response;
                }
                
                
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }

        }


        [HttpPost]
        public HttpResponseMessage delete(decimal id)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    var finduser = db.cls_usermst_tbl.Where(x => x.user_id == id).FirstOrDefault();
                    if (finduser != null)
                    {
                        db.Entry(finduser).State = EntityState.Deleted;
                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.OK, "Record deleted successfully");
                            return response;

                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.OK, "Failed to delete record");
                            return response;

                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, "Record not found");
                        return response;
                    }
                }

            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }

        }

        public HttpResponseMessage get()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    List<registrationmodel> userlist = db.cls_usermst_tbl.Select(x=> new registrationmodel
                    {
                        id = x.user_id,
                        empcode = x.user_code,
                        fullname = x.user_fullname,
                        email = x.user_emailid,
                        username = x.user_username,
                        department = x.user_department,
                        phone = x.user_phone,
                        role = x.user_role
                    }).ToList();
                    if (userlist.Count > 0)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, userlist);
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, "Record not found");
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message.ToString());
                return response;
            }
        }
    }
}
