using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsagreeController : ApiController
    {
        //create
        public HttpResponseMessage post(agreemodel agreemodel)
        {
            try
            {
               // DoMethodAsync();
                using (var db = new CompanyFormationdbEntities())
                {
                    HttpResponseMessage response = new HttpResponseMessage();



                    //var checkeckname = db.cls_agree_tbl.Where(x => x.name.ToLower() == agreemodel.name.ToLower()).FirstOrDefault();
                    //if (checkeckname == null)
                    //{
                    //    var checkcompanyname = db.cls_agree_tbl.Where(x => x.companyname.ToLower() == agreemodel.companyname.ToLower()).FirstOrDefault();
                    //    if (checkcompanyname == null)
                    //    {
                    //        var checkemail = db.cls_agree_tbl.Where(x => x.email.ToLower() == agreemodel.email.ToLower()).FirstOrDefault();
                    //        if (checkemail == null)
                    //        {

                    //            var checkphone = db.cls_agree_tbl.Where(x => x.phonenumber == agreemodel.phone).FirstOrDefault();
                    //            if (checkphone == null)
                    //            {
                    #region clsagree
                    cls_agree_tbl tbl;
                    tbl = db.cls_agree_tbl.Where(x => x.cfid == agreemodel.cfid).FirstOrDefault();
                    if (tbl != null)
                    {
                        tbl.agree = agreemodel.agree;
                        tbl.companypacktype = agreemodel.companypacktype;
                        tbl.incorporationtype = agreemodel.incorporationtype;
                        tbl.nonthirdparties = agreemodel.nonthirdparties;
                        tbl.paymenttype = agreemodel.paymenttype;
                        tbl.name = agreemodel.name;
                        tbl.companyname = agreemodel.companyname.Trim();
                        tbl.addressline1 = agreemodel.addressline1;
                        tbl.addressline2 = agreemodel.addressline2;
                        tbl.addressline3 = agreemodel.addressline3;
                        tbl.postcode = agreemodel.postal;
                        tbl.phonenumber = agreemodel.phone;
                        tbl.email = agreemodel.email;
                        tbl.username = agreemodel.username;

                        db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        response = Request.CreateResponse(HttpStatusCode.Created, "Success");
                    }
                    else
                    {

                        Dictionary<string, decimal> keys = new Dictionary<string, decimal>();
                        tbl = new cls_agree_tbl();

                        tbl.agree = agreemodel.agree;
                        tbl.companypacktype = agreemodel.companypacktype;
                        tbl.incorporationtype = agreemodel.incorporationtype;
                        tbl.nonthirdparties = agreemodel.nonthirdparties;
                        tbl.paymenttype = agreemodel.paymenttype;
                        tbl.name = agreemodel.name;
                        tbl.companyname = agreemodel.companyname.Trim();
                        tbl.addressline1 = agreemodel.addressline1;
                        tbl.addressline2 = agreemodel.addressline2;
                        tbl.addressline3 = agreemodel.addressline3;
                        tbl.postcode = agreemodel.postal;
                        tbl.phonenumber = agreemodel.phone;
                        tbl.email = agreemodel.email;
                        tbl.username = agreemodel.username;

                        db.cls_agree_tbl.Add(tbl);
                        db.SaveChanges();
                        db.Entry(tbl).GetDatabaseValues();
                        var cfid = tbl.cfid;
                        keys.Add("cfid", cfid);
                        response = Request.CreateResponse(HttpStatusCode.Created, keys);
                    }
                    #endregion clsagree
                    //            }
                    //            else
                    //            {
                    //                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Phone Number '" + agreemodel.phone + "' already exists.");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Email '" + agreemodel.email + "' already exists.");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Name '" + agreemodel.companyname + "' already exists.");
                    //    }
                    //}
                    //else
                    //{
                    //    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Name '" + agreemodel.name + "' already exists.");

                    //}
                    return response;
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }


        //Edit 
        public HttpResponseMessage put(agreemodel agreemodel)
        {
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    //var checkeckname = db.cls_agree_tbl.Where(x => x.name.ToLower() == agreemodel.name.ToLower()).FirstOrDefault();
                    //if (checkeckname == null)
                    //{
                    //    var checkcompanyname = db.cls_agree_tbl.Where(x => x.companyname.ToLower() == agreemodel.companyname.ToLower()).FirstOrDefault();
                    //    if (checkcompanyname == null)
                    //    {
                    //        var checkemail = db.cls_agree_tbl.Where(x => x.email.ToLower() == agreemodel.email.ToLower()).FirstOrDefault();
                    //        if (checkemail == null)
                    //        {

                    //            var checkphone = db.cls_agree_tbl.Where(x => x.phonenumber == agreemodel.phone).FirstOrDefault();
                    //            if (checkphone == null)
                    //            {
                    #region clsagree

                    // Dictionary<string, decimal> keys = new Dictionary<string, decimal>();
                    cls_agree_tbl tbl = db.cls_agree_tbl.Where(x => x.cfid == agreemodel.cfid).FirstOrDefault();
                    if (tbl != null)
                    {
                        tbl.agree = agreemodel.agree;
                        tbl.companypacktype = agreemodel.companypacktype;
                        tbl.incorporationtype = agreemodel.incorporationtype;
                        tbl.nonthirdparties = agreemodel.nonthirdparties;
                        tbl.paymenttype = agreemodel.paymenttype;
                        tbl.name = agreemodel.name;
                        tbl.companyname = agreemodel.companyname.Trim();
                        tbl.addressline1 = agreemodel.addressline1;
                        tbl.addressline2 = agreemodel.addressline2;
                        tbl.addressline3 = agreemodel.addressline3;
                        tbl.postcode = agreemodel.postal;
                        tbl.phonenumber = agreemodel.phone;
                        tbl.email = agreemodel.email;
                        tbl.username = agreemodel.username;

                        db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        response = Request.CreateResponse(HttpStatusCode.Created, "Updated");
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.Created, "Fail");
                    }
                    //    db.Entry(tbl).GetDatabaseValues();
                    //var cfid = tbl.cfid;
                    //keys.Add("cfid", cfid);

                    #endregion clsagree
                    //            }
                    //            else
                    //            {
                    //                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Phone Number '" + agreemodel.phone + "' already exists.");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Email '" + agreemodel.email + "' already exists.");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Company Name '" + agreemodel.companyname + "' already exists.");
                    //    }
                    //}
                    //else
                    //{
                    //    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Name '" + agreemodel.name + "' already exists.");

                    //}
                    return response;
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }

        protected void deleteclsagree(decimal cfid)
        {
            using (var db = new CompanyFormationdbEntities())
            {

                db.cls_agree_tbl.RemoveRange(db.cls_agree_tbl.Where(x => x.cfid == cfid));
                db.SaveChanges();

            }
        }

        /*
         * 16-09-2021
         * Only one folder and pdf file created in drive so no need to remove file code 
         * comment DoMethodAsync code
         */
        async Task DoMethodAsync()
        {
            try
            {
                WriteToFile(System.DateTime.Now.ToString());
                using (var db = new CompanyFormationdbEntities())
                {
                    var listcfid = db.cls_filemst_tbl.ToList();
                    if (listcfid != null)
                    {
                        foreach (var item in listcfid)
                        {
                            var filetoremove = db.cls_statusmst_tbl.Where(c => c.cfid == item.cfid).FirstOrDefault();
                            DateTime d1 = Convert.ToDateTime(filetoremove.createdon);
                            DateTime d2 = System.DateTime.Now;
                            TimeSpan diff = d2 - d1;
                            int d = diff.Days;
                            int h = diff.Hours;
                            int m = diff.Minutes;
                            
                            if (diff.Hours>=2 && diff.Minutes>0)
                            {
                                if (filetoremove.form_status == "Completed")
                                {
                                    var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/OneDrive - CLS Chartered Secretaries/clscharteredsecretaries/" + filetoremove.company_name + "/In Progress"), filetoremove.pdf_filename);
                                    var exists = File.Exists(filePath);
                                    if (exists)
                                    {
                                        File.Delete(filePath);
                                        cls_filemst_tbl tbl = db.cls_filemst_tbl.Where(c => c.cfid == filetoremove.cfid).FirstOrDefault();
                                        db.cls_filemst_tbl.Remove(tbl);
                                        db.SaveChanges();
                                        WriteToFile(filetoremove.cfid.ToString() + "File deleted Successfully");
                                    }
                                }
                                if (filetoremove.form_status == "InProgress")
                                {
                                    var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/OneDrive - CLS Chartered Secretaries/clscharteredsecretaries/" + filetoremove.company_name + "/Submit"), filetoremove.pdf_filename);
                                    var exists = File.Exists(filePath);
                                    if (exists)
                                    {
                                        File.Delete(filePath);
                                        cls_filemst_tbl tbl = db.cls_filemst_tbl.Where(c => c.cfid == filetoremove.cfid).FirstOrDefault();
                                        db.cls_filemst_tbl.Remove(tbl);
                                        db.SaveChanges();
                                        WriteToFile(filetoremove.cfid.ToString() + "File deleted Successfully");
                                    }
                                }
                                if (filetoremove.form_status == "Submit")
                                {
                                    var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/OneDrive - CLS Chartered Secretaries/clscharteredsecretaries/" + filetoremove.company_name + "/In Progress"), filetoremove.pdf_filename);

                                    var exists = File.Exists(filePath);
                                    if (exists)
                                    {
                                        File.Delete(filePath);
                                        cls_filemst_tbl tbl = db.cls_filemst_tbl.Where(c => c.cfid == filetoremove.cfid).FirstOrDefault();
                                        db.cls_filemst_tbl.Remove(tbl);
                                        db.SaveChanges();
                                        WriteToFile(filetoremove.cfid.ToString() + "File deleted Successfully");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("Exception");
                WriteToFile(ex.Message.ToString());
                WriteToFile(ex.InnerException.Message.ToString());
            }
            finally
            {
                WriteToFile("---------------------------------------------------------");
            }
        }

        #region log

        private void WriteToFile(string text)
        {
            string logFileName = "fileremove" + System.DateTime.Now.ToString("dd/MM/yyyy").Replace('/', '_').ToString() + ".log";
            var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Logs/"));
            if (!exists)
            {
                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/Logs/"));
            }
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Logs/" + logFileName);
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }


        #endregion
    }
}
