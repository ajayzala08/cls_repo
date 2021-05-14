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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clspasswordController : ApiController
    {
        public HttpResponseMessage Post(passwordresetmodel model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    var finduser = db.cls_usermst_tbl.Where(x => x.user_username == model.username && x.user_pwd == model.oldpwd).FirstOrDefault();
                    if (finduser != null)
                    {
                        finduser.user_pwd = model.newpwd;
                        db.Entry(finduser).State = EntityState.Modified;
                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.OK, "Password changes successfully.");
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Fail to change password.");
                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Old password not match.");
                    }
                }


                return response;
            }
            catch (Exception ex)
            {
                 response = Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }
        

        }
    }
}
