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
    public class clscompanyincorporationController : ApiController
    {
        public HttpResponseMessage postclscompanyincorporation(companyincorporationdetailsmodel model)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    var checkfirstchoice = db.cls_companyincorporation_tbl.Where(x => x.firstchoice.ToLower() == model.firstchoice.ToLower()).FirstOrDefault();
                    if (checkfirstchoice == null)
                    {
                        var secondchoice = db.cls_companyincorporation_tbl.Where(x => x.secondchoice.ToLower() == model.secondchoice.ToLower()).FirstOrDefault();
                        if (secondchoice == null)
                        {
                            var thirdchoice = db.cls_companyincorporation_tbl.Where(x => x.thirdchoice.ToLower() == model.thirdchoice.ToLower()).FirstOrDefault();
                            if (thirdchoice == null)
                            {

                                cls_companyincorporation_tbl tbl = new cls_companyincorporation_tbl();
                                tbl.cfid = model.cfid;
                                tbl.firstchoice = model.firstchoice;
                                tbl.secondchoice = model.secondchoice;
                                tbl.thirdchoice = model.thirdchoice;
                                tbl.principalactivity = model.principalactivity;
                                tbl.additionalwording = model.additionwording;
                                tbl.companytype = model.companytype;
                                db.cls_companyincorporation_tbl.Add(tbl);
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
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Third Choice '" + model.thirdchoice + "' already exists.");
                            }
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Second Choice '" + model.secondchoice + "' already exists.");
                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "First Choice '" + model.firstchoice + "' already exists.");
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
