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
    public class clsloginController : ApiController
    {
        public HttpResponseMessage Post(loginmodel model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            responsemodel rsmodel = new responsemodel();
            usermode usermodel = new usermode();
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    var user = db.cls_usermst_tbl.Where(x => x.user_username == model.username && x.user_pwd == model.password && x.user_active == 0 && x.user_delete == 1).FirstOrDefault();
                    if (user != null)
                    {
                        string firstname = string.Empty, lastname = string.Empty;
                        string[] names = user.user_fullname.ToString().Split(' ');
                        if (names.Length > 2)
                        {
                            firstname = names[0];
                            lastname = names[2];
                        }
                        else
                        {
                            if (names.Length == 1)
                            {
                                firstname = names[0];
                                lastname = "";
                            }
                            else
                            {
                                firstname = names[0];
                                lastname = names[1];
                            }
                        }

                        LoginResponseModel loginResponse = new LoginResponseModel();
                        loginResponse.EmployeeCode = Convert.ToDecimal(user.user_code);
                        loginResponse.Firstname = firstname;
                        loginResponse.Lastname = lastname;
                        loginResponse.Role = user.user_role;
                        loginResponse.Username = user.user_username;
                        loginResponse.token = "eyj0exaioijkv1qilcjhbgcioijiuzi1nij9.eyjpc3mioijjb2rlcnrozw1lcyisimlhdci6mtu4nzm1njy0oswizxhwijoxotayodg5ndq5lcjhdwqioijjb2rlcnrozw1lcy5jb20ilcjzdwiioijzdxbwb3j0qgnvzgvydghlbwvzlmnvbsisimxhc3royw1lijoivgvzdcisikvtywlsijoic3vwcg9ydebjb2rlcnrozw1lcy5jb20ilcjsb2xlijoiqwrtaw4ilcjmaxjzde5hbwuioijtahjlexuifq.d-isMYoGH6Ah4i_dHxplgJNGtXTLEqZYvha_ULSJRFU";
                        loginResponse.auth = "true";
                        //usermodel.code = user.user_code;
                        //usermodel.fullname = user.user_fullname;
                        //usermodel.email = user.user_emailid;
                        //usermodel.phoneno = user.user_phone;
                        //usermodel.username = user.user_username;
                        //usermodel.department = user.user_department;
                        //usermodel.token = "";
                        rsmodel.message = "Success";
                        rsmodel.status = true;
                        rsmodel.data = loginResponse;
                        response = Request.CreateResponse(HttpStatusCode.OK, rsmodel);

                    }
                    else
                    {
                        LoginResponseModel loginResponse = new LoginResponseModel();
                        loginResponse.EmployeeCode = Convert.ToDecimal("0");
                        loginResponse.Firstname = "";
                        loginResponse.Lastname = "";
                        loginResponse.Role = "";
                        loginResponse.Username = "";
                        loginResponse.token = "";
                        loginResponse.auth = "false";
                        rsmodel.message = "Fail";
                        rsmodel.status = false;
                        rsmodel.data = loginResponse;
                        response = Request.CreateResponse(HttpStatusCode.OK, rsmodel);

                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                rsmodel.message = "Exception";
                rsmodel.status = false;
                rsmodel.data = null;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, rsmodel);
                return response;
            }

        }
    }
}
