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
    public class clsaddressdetailsController : ApiController
    {
        
        public HttpResponseMessage postclsaddressdetails(addressdetailsmodel model)
        {
            HttpResponseMessage respose = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    if (model.cfid > 0 && model.cfid.ToString() != "")
                    {
                        //bool checkroofficeaddress = checckaddress(model);
                        //if(checkroofficeaddress)
                        //{
                        //    bool checkcaofficeaddress = checkcaaddress(model);
                        //    if (checkcaofficeaddress)
                        //    {
                                cls_addressdetails_tbl tbl = new cls_addressdetails_tbl();
                                tbl.cfid = model.cfid;
                                tbl.roaddressline1 = model.roaddressline1;
                                tbl.roaddressline2 = model.roaddressline2;
                                tbl.oraddressline3 = model.roaddressline3;
                                tbl.ropostalcode = model.ropostalcode;
                                tbl.caaddressline1 = model.caaddressline1;
                                tbl.caaddressline2 = model.caaddressline2;
                                tbl.caaddressline3 = model.caaddressline3;
                                tbl.capostalcode = model.capostalcode;
                                tbl.roisalsothebusinessorcaaddress = model.roisalsothebusinessorcaaddress;
                                db.cls_addressdetails_tbl.Add(tbl);
                                var result = db.SaveChanges();
                                if (result > 0)
                                {
                                    respose = Request.CreateResponse(HttpStatusCode.Created, "Success");

                                }
                                else
                                {
                                    respose = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");

                                }
                        //        }
                        //    }
                        //    return respose;
                        }
                        //else
                        //{
                        //    respose = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
                        //    
                        //}
                        return respose;
                    }

            }
            catch (Exception ex)
            {
                respose = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return respose;
            }

        }

        //public bool checckaddress(addressdetailsmodel model)
        //{
        //    bool checkaddresscondition = false;
        //    using (var db =new CompanyFormation_dbEntities())
        //    {
        //        if ((model.roaddressline1.ToString() != "") && (model.roaddressline1 != null))
        //        {
        //            var checckaddress = db.cls_addressdetails_tbl.Where(x => x.roaddressline1.ToLower() == model.roaddressline1.ToLower()
        //                    && x.roaddressline2.ToLower() == model.roaddressline2.ToLower() && x.oraddressline3.ToLower() == model.roaddressline3.ToLower()
        //                    && x.ropostalcode == model.ropostalcode
        //                    ).FirstOrDefault();
        //            if (checckaddress != null)
        //            {

        //                checkaddresscondition = false;

        //            }
        //            else
        //            {
        //                checkaddresscondition = true;
        //            }
        //        }
        //        else
        //        {
        //            checkaddresscondition = true;
        //        }
        //    }
            

        //    return checkaddresscondition;
        //}

        //public bool checkcaaddress(addressdetailsmodel model)
        //{
        //    bool checkaddresscondition = false;
        //    using (var db = new CompanyFormation_dbEntities())
        //    {
        //        if ((model.caaddressline1.ToString() != "") && (model.caaddressline1 != null))
        //        {
        //            var checkcaaddress = db.cls_addressdetails_tbl.Where(x => x.caaddressline1.ToLower() == model.caaddressline1.ToLower()
        //                  && x.caaddressline2.ToLower() == model.caaddressline2.ToLower() && x.caaddressline3.ToLower() == model.caaddressline3.ToLower()
        //                  && x.capostalcode == model.capostalcode
        //                  ).FirstOrDefault();
        //            if (checkcaaddress != null)
        //            {

        //                checkaddresscondition = false;

        //            }
        //            else
        //            {
        //                checkaddresscondition = true;
        //            }
        //        }
        //        else
        //        {
        //            checkaddresscondition = true;
        //        }
        //    }


        //    return checkaddresscondition;
        //}
    }
}
