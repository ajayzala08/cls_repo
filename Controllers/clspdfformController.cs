using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Http.Cors;


namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clspdfformController : ApiController
    {
        
        string companyFolderName = string.Empty;
        string pdfFileName = string.Empty;
        
        Dictionary<string,string> dictionary= new Dictionary<string, string>();
        public HttpResponseMessage post(decimal cfid)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                //  CreatePDFBody(cfid);
                //  string htmlstring = PopulateBody(cfid);
                string filepath = CreatePDFBody(cfid); //ExportToHtmlPdf(htmlstring);
                dictionary.Add("Filepath", filepath);
                dictionary.Add("CompanyName", companyFolderName);
                bool emailsendattachment = emailsend(companyFolderName, filepath, cfid);
                pdffomrstatus(cfid, companyFolderName, pdfFileName, filepath, "Submit");
                //response = Request.CreateResponse(HttpStatusCode.OK, Newtonsoft.Json.JsonConvert.SerializeObject(dictionary));
                response = Request.CreateResponse(HttpStatusCode.OK, dictionary);
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Exception. " + ex.InnerException.InnerException.Message.ToString());
            }
            return response;
        }
        private string ExportToHtmlPdf(string FormHtml)
        {
            byte[] bytes;
            //using (MemoryStream stream = new System.IO.MemoryStream())
            //{
            //    StringReader sr = new StringReader(FormHtml);
            //    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            //    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
            //    pdfDoc.Open();
            //    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
            //    pdfDoc.Close();
            //    bytes = stream.ToArray();
            //}
            //var filename = Guid.NewGuid();
            //string filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/PdfForm/" + filename + ".pdf");
            //System.IO.File.WriteAllBytes(filePath, bytes);
            //return true;


            //byte[] bPDF = null;



            MemoryStream ms = new MemoryStream();
            //StringReader txtReader = new StringReader(pHTML);
            TextReader txtReader = new StringReader(FormHtml);

            // 1: create object of a itextsharp document class
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);



            // 2: we create a itextsharp pdfwriter that listens to the document and directs a XML-stream to a file
            PdfWriter oPdfWriter = PdfWriter.GetInstance(doc, ms);

            // 3: we create a worker parse the document
            HTMLWorker htmlWorker = new HTMLWorker(doc);

            // 4: we open document and start the worker on the document
            doc.Open();
            doc.NewPage();
            htmlWorker.StartDocument();

            // 5: parse the html into the document
            htmlWorker.Parse(txtReader);

            // 6: close the document and the worker
            htmlWorker.EndDocument();
            htmlWorker.Close();
            doc.Close();

            bytes = ms.ToArray();

            var filename = Guid.NewGuid();
            pdfFileName = filename.ToString() + ".pdf";
            var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName + "/Submit"));
            if (!exists)
            {
                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName + "/Submit"));
            }

            string filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName + "/Submit/" + filename + ".pdf");
            System.IO.File.WriteAllBytes(filePath, bytes);
            return filePath;
        }
        private void pdffomrstatus(decimal cfid, string companyname, string filename, string filepath, string status)
        {
            using (var db = new CompanyFormationdbEntities())
            {
                cls_statusmst_tbl tbl;
                tbl = db.cls_statusmst_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                if (tbl != null)
                {
                    tbl.company_name = companyname;
                    tbl.pdf_filename = filename;
                    tbl.pdf_filepath = filepath;
                    tbl.form_status = status;
                    tbl.createdon = DateTime.Now;
                    db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {

                    tbl = new cls_statusmst_tbl();
                    tbl.cfid = cfid;
                    tbl.company_name = companyname;
                    tbl.pdf_filename = filename;
                    tbl.pdf_filepath = filepath;
                    tbl.form_status = status;
                    tbl.createdon = DateTime.Now;
                    db.cls_statusmst_tbl.Add(tbl);
                    db.SaveChanges();
                }

            }
        }
        private string PopulateBody(decimal cfid)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/TemplateFile/Formpdf.html");
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(path))
            {
                body = reader.ReadToEnd();
            }
            using (var db = new CompanyFormationdbEntities())
            {
                var section1 = db.cls_agree_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                
                if (section1.agree.ToString() == "0")
                {
                    body = body.Replace("{Agree}", "Agree");
                }
                else
                {
                    body = body.Replace("{Agree}", "DisAgree");
                }

                body = body.Replace("{ThirdParty}", section1.nonthirdparties != null ? section1.nonthirdparties.ToString() : "");
                body = body.Replace("{Standard}", section1.incorporationtype != null ? section1.incorporationtype.ToString() : "");
                body = body.Replace("{packtype}", section1.companypacktype != null ? section1.companypacktype.ToString() : "");
                body = body.Replace("{paymenttype}", section1.paymenttype != null ? section1.paymenttype.ToString() : "");

                body = body.Replace("{name}", section1.name != null ? section1.name.ToString() : "");
                body = body.Replace("{companyname}", section1.companyname != null ? section1.companyname.ToString() : "");
                body = body.Replace("{companyaddressline1}", section1.addressline1 != null ? section1.addressline1.ToString() : "");
                body = body.Replace("{companyaddressline2}", section1.addressline2 != null ? section1.addressline2.ToString() : "");
                body = body.Replace("{companyaddressline3}", section1.addressline3 != null ? section1.addressline3.ToString() : "");
                body = body.Replace("{companypostcode}", section1.postcode != null ? section1.postcode.ToString() : "");
                body = body.Replace("{companyphonenumber}", section1.phonenumber != null ? section1.phonenumber.ToString() : "");
                body = body.Replace("{companyemail}", section1.email != null ? section1.email.ToString() : "");
                companyFolderName = section1.companyname != null ? section1.companyname.ToString() : "Temp";
                var section2 = db.cls_companyincorporation_tbl.Where(x => x.cfid == cfid).FirstOrDefault();

                if (section2 != null)
                {
                    body = body.Replace("{firstchoice}", section2.firstchoice != null ? section2.firstchoice.ToString() : "");
                    body = body.Replace("{secondchoice}", section2.secondchoice != null ? section2.secondchoice.ToString() : "");
                    body = body.Replace("{thirdchoice}", section2.thirdchoice != null ? section2.thirdchoice.ToString() : "");
                    body = body.Replace("{principalactivity}", section2.principalactivity != null ? section2.principalactivity.ToString() : "");
                    body = body.Replace("{additionalwording}", section2.additionalwording != null ? section2.additionalwording.ToString() : "");
                    body = body.Replace("{companytype}", section2.companytype != null ? section2.companytype.ToString() : "");
                }
                else
                {
                    body = body.Replace("{firstchoice}", "");
                    body = body.Replace("{secondchoice}", "");
                    body = body.Replace("{thirdchoice}", "");
                    body = body.Replace("{principalactivity}", "");
                    body = body.Replace("{additionalwording}", "");
                    body = body.Replace("{companytype}", "");
                }

                var section3 = db.cls_sharecapital_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                if (section3 != null)
                {
                    body = body.Replace("{sharecapital}", section3.issuedsharecapital != null ? section3.issuedsharecapital.ToString() : "");
                    body = body.Replace("{nominalamount}", section3.nominalamoutpershare != null ? section3.nominalamoutpershare.ToString() : "");
                    body = body.Replace("{shareclass}", section3.shareclass != null ? section3.shareclass.ToString() : "");
                    body = body.Replace("{authorisedsharecapital}", section3.authorisedsharecapital != null ? section3.authorisedsharecapital.ToString() : "");
                }
                else
                {
                    body = body.Replace("{sharecapital}", "");
                    body = body.Replace("{nominalamount}", "");
                    body = body.Replace("{shareclass}", "");
                    body = body.Replace("{authorisedsharecapital}", "");
                }
                var section4 = db.cls_secretary_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                if (section4 != null)
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    body = body.Replace("{isname}", section4.name != null ? section4.name.ToString() : "");
                    body = body.Replace("{isdob}", section4.dob != null ? Convert.ToDateTime(section4.dob).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                    body = body.Replace("{isaddressline1}", section4.addressline1 != null ? section4.addressline1.ToString() : "");
                    body = body.Replace("{isaddressline2}", section4.addressline2 != null ? section4.addressline2.ToString() : "");
                    body = body.Replace("{isaddressline3}", section4.addressline3 != null ? section4.addressline3.ToString() : "");
                    body = body.Replace("{ispostcode}", section4.postalcode != null ? section4.postalcode.ToString() : "");
                    body = body.Replace("{iscountry}", section4.country != null ? section4.country.ToString() : "");

                    body = body.Replace("{csname}", section4.companyname != null ? section4.companyname.ToString() : "");
                    body = body.Replace("{csnumber}", section4.companynumber != null ? section4.companynumber.ToString() : "");
                    body = body.Replace("{csdirector}", section4.companydirector != null ? section4.companydirector.ToString() : "");
                    body = body.Replace("{csrooffice}", section4.companyregionaloffice != null ? section4.companyregionaloffice.ToString() : "");
                    body = body.Replace("{csaddressline1}", section4.companyaddressline1 != null ? section4.companyaddressline1.ToString() : "");
                    body = body.Replace("{csaddressline2}", section4.companyaddressline2 != null ? section4.companyaddressline2.ToString() : "");
                    body = body.Replace("{cspostcode}", section4.compnaypostal != null ? section4.compnaypostal.ToString() : "");
                    body = body.Replace("{cscountry}", section4.compnaycountry != null ? section4.compnaycountry.ToString() : "");
                }
                else
                {
                    body = body.Replace("{isname}", "");
                    body = body.Replace("{isdob}", "");
                    body = body.Replace("{isaddressline1}", "");
                    body = body.Replace("{isaddressline2}", "");
                    body = body.Replace("{isaddressline3}", "");
                    body = body.Replace("{ispostcode}", "");
                    body = body.Replace("{iscountry}", "");

                    body = body.Replace("{csname}", "");
                    body = body.Replace("{csnumber}", "");
                    body = body.Replace("{csdirector}", "");
                    body = body.Replace("{csrooffice}", "");
                    body = body.Replace("{csaddressline1}", "");
                    body = body.Replace("{csaddressline2}", "");
                    body = body.Replace("{cspostcode}", "");
                    body = body.Replace("{cscountry}", "");
                }


                var section5 = db.cls_director_tbl.Where(x => x.cfid == cfid).ToList();
                if (section5 != null)
                {
                    if (section5.Count > 1)
                    {
                        body = body.Replace("{directorname1}", section5[0].name != null ? section5[0].name.ToString() : "");
                        body = body.Replace("{directordob1}", section5[0].dob != null ? Convert.ToDateTime(section5[0].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation1}", section5[0].occupation != null ? section5[0].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline11}", section5[0].addressline1 != null ? section5[0].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline21}", section5[0].addressline2 != null ? section5[0].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline31}", section5[0].addressline3 != null ? section5[0].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode1}", section5[0].postalcode != null ? section5[0].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry1}", section5[0].country != null ? section5[0].country.ToString() : "");
                        body = body.Replace("{directornationality1}", section5[0].nationality != null ? section5[0].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship11}", section5[0].otherdirectorship1 != null ? section5[0].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship21}", section5[0].otherdirectorship2 != null ? section5[0].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship31}", section5[0].otherdirectorship3 != null ? section5[0].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified1}", section5[0].restricted != null ? section5[0].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber1}", section5[0].numberofshare != null ? section5[0].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner1}", section5[0].beneficialowner != null ? section5[0].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname1}", "");
                        body = body.Replace("{directordob1}", "");
                        body = body.Replace("{directoroccupation1}", "");
                        body = body.Replace("{directoraddressline11}", "");
                        body = body.Replace("{directoraddressline21}", "");
                        body = body.Replace("{directoraddressline31}", "");
                        body = body.Replace("{directorpostcode1}", "");
                        body = body.Replace("{directorcountry1}", "");
                        body = body.Replace("{directornationality1}", "");
                        body = body.Replace("{directorotherdirectorship11}", "");
                        body = body.Replace("{directorotherdirectorship21}", "");
                        body = body.Replace("{directorotherdirectorship31}", "");
                        body = body.Replace("{directordisqualified1}", "");
                        body = body.Replace("{directorsubscriber1}", "");
                        body = body.Replace("{directorbeneficialowner1}", "");
                    }

                    if (section5.Count >= 2)
                    {
                        body = body.Replace("{directorname2}", section5[1].name != null ? section5[1].name.ToString() : "");
                        body = body.Replace("{directordob2}", section5[1].dob != null ? Convert.ToDateTime(section5[1].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation2}", section5[1].occupation != null ? section5[1].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline12}", section5[1].addressline1 != null ? section5[1].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline22}", section5[1].addressline2 != null ? section5[1].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline32}", section5[1].addressline3 != null ? section5[1].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode2}", section5[1].postalcode != null ? section5[1].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry2}", section5[1].country != null ? section5[1].country.ToString() : "");
                        body = body.Replace("{directornationality2}", section5[1].nationality != null ? section5[1].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship12}", section5[1].otherdirectorship1 != null ? section5[1].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship22}", section5[1].otherdirectorship2 != null ? section5[1].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship32}", section5[1].otherdirectorship3 != null ? section5[1].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified2}", section5[1].restricted != null ? section5[1].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber2}", section5[1].numberofshare != null ? section5[1].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner2}", section5[1].beneficialowner != null ? section5[1].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname2}", "");
                        body = body.Replace("{directordob2}", "");
                        body = body.Replace("{directoroccupation2}", "");
                        body = body.Replace("{directoraddressline12}", "");
                        body = body.Replace("{directoraddressline22}", "");
                        body = body.Replace("{directoraddressline32}", "");
                        body = body.Replace("{directorpostcode2}", "");
                        body = body.Replace("{directorcountry2}", "");
                        body = body.Replace("{directornationality2}", "");
                        body = body.Replace("{directorotherdirectorship12}", "");
                        body = body.Replace("{directorotherdirectorship22}", "");
                        body = body.Replace("{directorotherdirectorship32}", "");
                        body = body.Replace("{directordisqualified2}", "");
                        body = body.Replace("{directorsubscriber2}", "");
                        body = body.Replace("{directorbeneficialowner2}", "");
                    }

                    if (section5.Count >= 3)
                    {
                        body = body.Replace("{directorname3}", section5[2].name != null ? section5[2].name.ToString() : "");
                        body = body.Replace("{directordob3}", section5[2].dob != null ? Convert.ToDateTime(section5[2].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation3}", section5[2].occupation != null ? section5[2].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline13}", section5[2].addressline1 != null ? section5[2].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline23}", section5[2].addressline2 != null ? section5[2].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline33}", section5[2].addressline3 != null ? section5[2].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode3}", section5[2].postalcode != null ? section5[2].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry3}", section5[2].country != null ? section5[2].country.ToString() : "");
                        body = body.Replace("{directornationality3}", section5[2].nationality != null ? section5[2].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship13}", section5[2].otherdirectorship1 != null ? section5[2].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship23}", section5[2].otherdirectorship2 != null ? section5[2].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship33}", section5[2].otherdirectorship3 != null ? section5[2].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified3}", section5[2].restricted != null ? section5[2].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber3}", section5[2].numberofshare != null ? section5[2].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner3}", section5[2].beneficialowner != null ? section5[2].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname3}", "");
                        body = body.Replace("{directordob3}", "");
                        body = body.Replace("{directoroccupation3}", "");
                        body = body.Replace("{directoraddressline13}", "");
                        body = body.Replace("{directoraddressline23}", "");
                        body = body.Replace("{directoraddressline33}", "");
                        body = body.Replace("{directorpostcode3}", "");
                        body = body.Replace("{directorcountry3}", "");
                        body = body.Replace("{directornationality3}", "");
                        body = body.Replace("{directorotherdirectorship13}", "");
                        body = body.Replace("{directorotherdirectorship23}", "");
                        body = body.Replace("{directorotherdirectorship33}", "");
                        body = body.Replace("{directordisqualified3}", "");
                        body = body.Replace("{directorsubscriber3}", "");
                        body = body.Replace("{directorbeneficialowner3}", "");
                    }

                    if (section5.Count >= 4)
                    {
                        body = body.Replace("{directorname4}", section5[3].name != null ? section5[3].name.ToString() : "");
                        body = body.Replace("{directordob4}", section5[3].dob != null ? Convert.ToDateTime(section5[3].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation4}", section5[3].occupation != null ? section5[3].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline14}", section5[3].addressline1 != null ? section5[3].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline24}", section5[3].addressline2 != null ? section5[3].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline34}", section5[3].addressline3 != null ? section5[3].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode4}", section5[3].postalcode != null ? section5[3].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry4}", section5[3].country != null ? section5[3].country.ToString() : "");
                        body = body.Replace("{directornationality4}", section5[3].nationality != null ? section5[3].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship14}", section5[3].otherdirectorship1 != null ? section5[3].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship24}", section5[3].otherdirectorship2 != null ? section5[3].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship34}", section5[3].otherdirectorship3 != null ? section5[3].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified4}", section5[3].restricted != null ? section5[3].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber4}", section5[3].numberofshare != null ? section5[3].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner4}", section5[3].beneficialowner != null ? section5[3].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname4}", "");
                        body = body.Replace("{directordob4}", "");
                        body = body.Replace("{directoroccupation4}", "");
                        body = body.Replace("{directoraddressline14}", "");
                        body = body.Replace("{directoraddressline24}", "");
                        body = body.Replace("{directoraddressline34}", "");
                        body = body.Replace("{directorpostcode4}", "");
                        body = body.Replace("{directorcountry4}", "");
                        body = body.Replace("{directornationality4}", "");
                        body = body.Replace("{directorotherdirectorship14}", "");
                        body = body.Replace("{directorotherdirectorship24}", "");
                        body = body.Replace("{directorotherdirectorship34}", "");
                        body = body.Replace("{directordisqualified4}", "");
                        body = body.Replace("{directorsubscriber4}", "");
                        body = body.Replace("{directorbeneficialowner4}", "");
                    }

                    if (section5.Count >= 5)
                    {
                        body = body.Replace("{directorname5}", section5[4].name != null ? section5[4].name.ToString() : "");
                        body = body.Replace("{directordob5}", section5[4].dob != null ? Convert.ToDateTime(section5[4].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation5}", section5[4].occupation != null ? section5[4].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline15}", section5[4].addressline1 != null ? section5[4].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline25}", section5[4].addressline2 != null ? section5[4].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline35}", section5[4].addressline3 != null ? section5[4].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode5}", section5[4].postalcode != null ? section5[4].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry5}", section5[4].country != null ? section5[4].country.ToString() : "");
                        body = body.Replace("{directornationality5}", section5[4].nationality != null ? section5[4].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship15}", section5[4].otherdirectorship1 != null ? section5[4].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship25}", section5[4].otherdirectorship2 != null ? section5[4].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship35}", section5[4].otherdirectorship3 != null ? section5[4].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified5}", section5[4].restricted != null ? section5[4].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber5}", section5[4].numberofshare != null ? section5[4].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner5}", section5[4].beneficialowner != null ? section5[4].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname5}", "");
                        body = body.Replace("{directordob5}", "");
                        body = body.Replace("{directoroccupation5}", "");
                        body = body.Replace("{directoraddressline15}", "");
                        body = body.Replace("{directoraddressline25}", "");
                        body = body.Replace("{directoraddressline35}", "");
                        body = body.Replace("{directorpostcode5}", "");
                        body = body.Replace("{directorcountry5}", "");
                        body = body.Replace("{directornationality5}", "");
                        body = body.Replace("{directorotherdirectorship15}", "");
                        body = body.Replace("{directorotherdirectorship25}", "");
                        body = body.Replace("{directorotherdirectorship35}", "");
                        body = body.Replace("{directordisqualified5}", "");
                        body = body.Replace("{directorsubscriber5}", "");
                        body = body.Replace("{directorbeneficialowner5}", "");
                    }

                    if (section5.Count >= 6)
                    {
                        body = body.Replace("{directorname6}", section5[5].name != null ? section5[5].name.ToString() : "");
                        body = body.Replace("{directordob6}", section5[5].dob != null ? Convert.ToDateTime(section5[5].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation6}", section5[5].occupation != null ? section5[5].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline16}", section5[5].addressline1 != null ? section5[5].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline26}", section5[5].addressline2 != null ? section5[5].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline36}", section5[5].addressline3 != null ? section5[5].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode6}", section5[5].postalcode != null ? section5[5].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry6}", section5[5].country != null ? section5[5].country.ToString() : "");
                        body = body.Replace("{directornationality6}", section5[5].nationality != null ? section5[5].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship16}", section5[5].otherdirectorship1 != null ? section5[5].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship26}", section5[5].otherdirectorship2 != null ? section5[5].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship36}", section5[5].otherdirectorship3 != null ? section5[5].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified6}", section5[5].restricted != null ? section5[5].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber6}", section5[5].numberofshare != null ? section5[5].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner6}", section5[5].beneficialowner != null ? section5[5].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname6}", "");
                        body = body.Replace("{directordob6}", "");
                        body = body.Replace("{directoroccupation6}", "");
                        body = body.Replace("{directoraddressline16}", "");
                        body = body.Replace("{directoraddressline26}", "");
                        body = body.Replace("{directoraddressline36}", "");
                        body = body.Replace("{directorpostcode6}", "");
                        body = body.Replace("{directorcountry6}", "");
                        body = body.Replace("{directornationality6}", "");
                        body = body.Replace("{directorotherdirectorship16}", "");
                        body = body.Replace("{directorotherdirectorship26}", "");
                        body = body.Replace("{directorotherdirectorship36}", "");
                        body = body.Replace("{directordisqualified6}", "");
                        body = body.Replace("{directorsubscriber6}", "");
                        body = body.Replace("{directorbeneficialowner6}", "");
                    }

                    if (section5.Count >= 7)
                    {
                        body = body.Replace("{directorname7}", section5[6].name != null ? section5[6].name.ToString() : "");
                        body = body.Replace("{directordob7}", section5[6].dob != null ? Convert.ToDateTime(section5[6].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "");
                        body = body.Replace("{directoroccupation7}", section5[6].occupation != null ? section5[6].occupation.ToString() : "");
                        body = body.Replace("{directoraddressline17}", section5[6].addressline1 != null ? section5[6].addressline1.ToString() : "");
                        body = body.Replace("{directoraddressline27}", section5[6].addressline2 != null ? section5[6].addressline2.ToString() : "");
                        body = body.Replace("{directoraddressline37}", section5[6].addressline3 != null ? section5[6].addressline3.ToString() : "");
                        body = body.Replace("{directorpostcode7}", section5[6].postalcode != null ? section5[6].postalcode.ToString() : "");
                        body = body.Replace("{directorcountry7}", section5[6].country != null ? section5[6].country.ToString() : "");
                        body = body.Replace("{directornationality7}", section5[6].nationality != null ? section5[6].nationality.ToString() : "");
                        body = body.Replace("{directorotherdirectorship17}", section5[6].otherdirectorship1 != null ? section5[6].otherdirectorship1.ToString() : "");
                        body = body.Replace("{directorotherdirectorship27}", section5[6].otherdirectorship2 != null ? section5[6].otherdirectorship2.ToString() : "");
                        body = body.Replace("{directorotherdirectorship37}", section5[6].otherdirectorship3 != null ? section5[6].otherdirectorship3.ToString() : "");
                        body = body.Replace("{directordisqualified7}", section5[6].restricted != null ? section5[6].restricted.ToString() : "");
                        body = body.Replace("{directorsubscriber7}", section5[6].numberofshare != null ? section5[6].numberofshare.ToString() : "");
                        body = body.Replace("{directorbeneficialowner7}", section5[6].beneficialowner != null ? section5[6].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{directorname7}", "");
                        body = body.Replace("{directordob7}", "");
                        body = body.Replace("{directoroccupation7}", "");
                        body = body.Replace("{directoraddressline17}", "");
                        body = body.Replace("{directoraddressline27}", "");
                        body = body.Replace("{directoraddressline37}", "");
                        body = body.Replace("{directorpostcode7}", "");
                        body = body.Replace("{directorcountry7}", "");
                        body = body.Replace("{directornationality7}", "");
                        body = body.Replace("{directorotherdirectorship17}", "");
                        body = body.Replace("{directorotherdirectorship27}", "");
                        body = body.Replace("{directorotherdirectorship37}", "");
                        body = body.Replace("{directordisqualified7}", "");
                        body = body.Replace("{directorsubscriber7}", "");
                        body = body.Replace("{directorbeneficialowner7}", "");
                    }


                }

                var section6 = db.cls_subscriber_tbl.Where(x => x.cfid == cfid).ToList();
                if (section6 != null)
                {
                    if (section6.Count >= 1)
                    {
                        body = body.Replace("{subscribername1}", section6[0].name != null ? section6[0].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline11}", section6[0].addressline1 != null ? section6[0].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline21}", section6[0].addressline2 != null ? section6[0].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline31}", section6[0].addressline3 != null ? section6[0].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode1}", section6[0].postalcode != null ? section6[0].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry1}", section6[0].country != null ? section6[0].country.ToString() : "");
                        body = body.Replace("{subscribernationality1}", section6[0].nationality != null ? section6[0].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation1}", section6[0].occupation != null ? section6[0].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare1}", section6[0].numberofshare != null ? section6[0].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner1}", section6[0].beneficialowner != null ? section6[0].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername1}", "");
                        body = body.Replace("{subscriberaddressline11}", "");
                        body = body.Replace("{subscriberaddressline21}", "");
                        body = body.Replace("{subscriberaddressline31}", "");
                        body = body.Replace("{subscriberpostcode1}", "");
                        body = body.Replace("{subscribercountry1}", "");
                        body = body.Replace("{subscribernationality1}", "");
                        body = body.Replace("{subscriberoccupation1}", "");
                        body = body.Replace("{subscribernoofshare1}", "");
                        body = body.Replace("{subscriberbeneficialowner1}", "");
                    }

                    if (section6.Count >= 2)
                    {
                        body = body.Replace("{subscribername2}", section6[1].name != null ? section6[1].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline12}", section6[1].addressline1 != null ? section6[1].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline22}", section6[1].addressline2 != null ? section6[1].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline32}", section6[1].addressline3 != null ? section6[1].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode2}", section6[1].postalcode != null ? section6[1].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry2}", section6[1].country != null ? section6[1].country.ToString() : "");
                        body = body.Replace("{subscribernationality2}", section6[1].nationality != null ? section6[1].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation2}", section6[1].occupation != null ? section6[1].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare2}", section6[1].numberofshare != null ? section6[1].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner2}", section6[1].beneficialowner != null ? section6[1].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername2}", "");
                        body = body.Replace("{subscriberaddressline12}", "");
                        body = body.Replace("{subscriberaddressline22}", "");
                        body = body.Replace("{subscriberaddressline32}", "");
                        body = body.Replace("{subscriberpostcode2}", "");
                        body = body.Replace("{subscribercountry2}", "");
                        body = body.Replace("{subscribernationality2}", "");
                        body = body.Replace("{subscriberoccupation2}", "");
                        body = body.Replace("{subscribernoofshare2}", "");
                        body = body.Replace("{subscriberbeneficialowner2}", "");
                    }

                    if (section6.Count >= 3)
                    {
                        body = body.Replace("{subscribername3}", section6[2].name != null ? section6[2].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline13}", section6[2].addressline1 != null ? section6[2].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline23}", section6[2].addressline2 != null ? section6[2].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline33}", section6[2].addressline3 != null ? section6[2].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode3}", section6[2].postalcode != null ? section6[2].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry3}", section6[2].country != null ? section6[2].country.ToString() : "");
                        body = body.Replace("{subscribernationality3}", section6[2].nationality != null ? section6[2].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation3}", section6[2].occupation != null ? section6[2].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare3}", section6[2].numberofshare != null ? section6[2].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner3}", section6[2].beneficialowner != null ? section6[2].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername3}", "");
                        body = body.Replace("{subscriberaddressline13}", "");
                        body = body.Replace("{subscriberaddressline23}", "");
                        body = body.Replace("{subscriberaddressline33}", "");
                        body = body.Replace("{subscriberpostcode3}", "");
                        body = body.Replace("{subscribercountry3}", "");
                        body = body.Replace("{subscribernationality3}", "");
                        body = body.Replace("{subscriberoccupation3}", "");
                        body = body.Replace("{subscribernoofshare3}", "");
                        body = body.Replace("{subscriberbeneficialowner3}", "");
                    }

                    if (section6.Count >= 4)
                    {
                        body = body.Replace("{subscribername4}", section6[3].name != null ? section6[3].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline14}", section6[3].addressline1 != null ? section6[3].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline24}", section6[3].addressline2 != null ? section6[3].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline34}", section6[3].addressline3 != null ? section6[3].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode4}", section6[3].postalcode != null ? section6[3].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry4}", section6[3].country != null ? section6[3].country.ToString() : "");
                        body = body.Replace("{subscribernationality4}", section6[3].nationality != null ? section6[3].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation4}", section6[3].occupation != null ? section6[3].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare4}", section6[3].numberofshare != null ? section6[3].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner4}", section6[3].beneficialowner != null ? section6[3].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername4}", "");
                        body = body.Replace("{subscriberaddressline14}", "");
                        body = body.Replace("{subscriberaddressline24}", "");
                        body = body.Replace("{subscriberaddressline34}", "");
                        body = body.Replace("{subscriberpostcode4}", "");
                        body = body.Replace("{subscribercountry4}", "");
                        body = body.Replace("{subscribernationality4}", "");
                        body = body.Replace("{subscriberoccupation4}", "");
                        body = body.Replace("{subscribernoofshare4}", "");
                        body = body.Replace("{subscriberbeneficialowner4}", "");
                    }

                    if (section6.Count >= 4)
                    {
                        body = body.Replace("{subscribername4}", section6[3].name != null ? section6[3].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline14}", section6[3].addressline1 != null ? section6[3].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline24}", section6[3].addressline2 != null ? section6[3].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline34}", section6[3].addressline3 != null ? section6[3].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode4}", section6[3].postalcode != null ? section6[3].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry4}", section6[3].country != null ? section6[3].country.ToString() : "");
                        body = body.Replace("{subscribernationality4}", section6[3].nationality != null ? section6[3].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation4}", section6[3].occupation != null ? section6[3].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare4}", section6[3].numberofshare != null ? section6[3].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner4}", section6[3].beneficialowner != null ? section6[3].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername4}", "");
                        body = body.Replace("{subscriberaddressline14}", "");
                        body = body.Replace("{subscriberaddressline24}", "");
                        body = body.Replace("{subscriberaddressline34}", "");
                        body = body.Replace("{subscriberpostcode4}", "");
                        body = body.Replace("{subscribercountry4}", "");
                        body = body.Replace("{subscribernationality4}", "");
                        body = body.Replace("{subscriberoccupation4}", "");
                        body = body.Replace("{subscribernoofshare4}", "");
                        body = body.Replace("{subscriberbeneficialowner4}", "");
                    }

                    if (section6.Count >= 5)
                    {
                        body = body.Replace("{subscribername5}", section6[4].name != null ? section6[4].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline15}", section6[4].addressline1 != null ? section6[4].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline25}", section6[4].addressline2 != null ? section6[4].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline35}", section6[4].addressline3 != null ? section6[4].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode5}", section6[4].postalcode != null ? section6[4].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry5}", section6[4].country != null ? section6[4].country.ToString() : "");
                        body = body.Replace("{subscribernationality5}", section6[4].nationality != null ? section6[4].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation5}", section6[4].occupation != null ? section6[4].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare5}", section6[4].numberofshare != null ? section6[4].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner5}", section6[4].beneficialowner != null ? section6[4].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername5}", "");
                        body = body.Replace("{subscriberaddressline15}", "");
                        body = body.Replace("{subscriberaddressline25}", "");
                        body = body.Replace("{subscriberaddressline35}", "");
                        body = body.Replace("{subscriberpostcode5}", "");
                        body = body.Replace("{subscribercountry5}", "");
                        body = body.Replace("{subscribernationality5}", "");
                        body = body.Replace("{subscriberoccupation5}", "");
                        body = body.Replace("{subscribernoofshare5}", "");
                        body = body.Replace("{subscriberbeneficialowner5}", "");
                    }

                    if (section6.Count >= 6)
                    {
                        body = body.Replace("{subscribername6}", section6[5].name != null ? section6[5].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline16}", section6[5].addressline1 != null ? section6[5].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline26}", section6[5].addressline2 != null ? section6[5].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline36}", section6[5].addressline3 != null ? section6[5].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode6}", section6[5].postalcode != null ? section6[5].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry6}", section6[5].country != null ? section6[5].country.ToString() : "");
                        body = body.Replace("{subscribernationality6}", section6[5].nationality != null ? section6[5].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation6}", section6[5].occupation != null ? section6[5].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare6}", section6[5].numberofshare != null ? section6[5].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner6}", section6[5].beneficialowner != null ? section6[5].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername6}", "");
                        body = body.Replace("{subscriberaddressline16}", "");
                        body = body.Replace("{subscriberaddressline26}", "");
                        body = body.Replace("{subscriberaddressline36}", "");
                        body = body.Replace("{subscriberpostcode6}", "");
                        body = body.Replace("{subscribercountry6}", "");
                        body = body.Replace("{subscribernationality6}", "");
                        body = body.Replace("{subscriberoccupation6}", "");
                        body = body.Replace("{subscribernoofshare6}", "");
                        body = body.Replace("{subscriberbeneficialowner6}", "");
                    }

                    if (section6.Count >= 7)
                    {
                        body = body.Replace("{subscribername7}", section6[6].name != null ? section6[6].name.ToString() : "");
                        body = body.Replace("{subscriberaddressline17}", section6[6].addressline1 != null ? section6[6].addressline1.ToString() : "");
                        body = body.Replace("{subscriberaddressline27}", section6[6].addressline2 != null ? section6[6].addressline2.ToString() : "");
                        body = body.Replace("{subscriberaddressline37}", section6[6].addressline3 != null ? section6[6].addressline3.ToString() : "");
                        body = body.Replace("{subscriberpostcode7}", section6[6].postalcode != null ? section6[6].postalcode.ToString() : "");
                        body = body.Replace("{subscribercountry7}", section6[6].country != null ? section6[6].country.ToString() : "");
                        body = body.Replace("{subscribernationality7}", section6[6].nationality != null ? section6[6].nationality.ToString() : "");
                        body = body.Replace("{subscriberoccupation7}", section6[6].occupation != null ? section6[6].occupation.ToString() : "");
                        body = body.Replace("{subscribernoofshare7}", section6[6].numberofshare != null ? section6[6].numberofshare.ToString() : "");
                        body = body.Replace("{subscriberbeneficialowner7}", section6[6].beneficialowner != null ? section6[6].beneficialowner.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{subscribername7}", "");
                        body = body.Replace("{subscriberaddressline17}", "");
                        body = body.Replace("{subscriberaddressline27}", "");
                        body = body.Replace("{subscriberaddressline37}", "");
                        body = body.Replace("{subscriberpostcode7}", "");
                        body = body.Replace("{subscribercountry7}", "");
                        body = body.Replace("{subscribernationality7}", "");
                        body = body.Replace("{subscriberoccupation7}", "");
                        body = body.Replace("{subscribernoofshare7}", "");
                        body = body.Replace("{subscriberbeneficialowner7}", "");
                    }
                }

                var section7 = db.cls_corporatesubscriber_tbl.Where(x => x.cfid == cfid).ToList();
                if (section7 != null)
                {
                    if (section7.Count >= 1)
                    {
                        body = body.Replace("{cscompanyname1}", section7[0].companyname != null ? section7[0].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber1}", section7[0].companyphonenumber != null ? section7[0].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector1}", section7[0].companydirector != null ? section7[0].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline11}", section7[0].registerofficeaddress != null ? section7[0].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline21}", section7[0].addressline2 != null ? section7[0].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline31}", section7[0].addressline3 != null ? section7[0].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode1}", section7[0].postalcode != null ? section7[0].postalcode.ToString() : "");
                        body = body.Replace("{cscountry1}", section7[0].country != null ? section7[0].country : "");
                        body = body.Replace("{csnoofshare1}", section7[0].numberofshare != null ? section7[0].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname1}", "");
                        body = body.Replace("{cscompanynumber1}", "");
                        body = body.Replace("{cscompanydirector1}", "");
                        body = body.Replace("{csaddressline11}", "");
                        body = body.Replace("{csaddressline21}", "");
                        body = body.Replace("{csaddressline31}", "");
                        body = body.Replace("{cspostcode1}", "");
                        body = body.Replace("{cscountry1}", "");
                        body = body.Replace("{csnoofshare1}", "");
                    }

                    if (section7.Count >= 2)
                    {
                        body = body.Replace("{cscompanyname2}", section7[1].companyname != null ? section7[1].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber2}", section7[1].companyphonenumber != null ? section7[1].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector2}", section7[1].companydirector != null ? section7[1].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline12}", section7[1].registerofficeaddress != null ? section7[1].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline22}", section7[1].addressline2 != null ? section7[1].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline32}", section7[1].addressline3 != null ? section7[1].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode2}", section7[1].postalcode != null ? section7[1].postalcode.ToString() : "");
                        body = body.Replace("{cscountry2}", section7[1].country != null ? section7[1].country : "");
                        body = body.Replace("{csnoofshare2}", section7[1].numberofshare != null ? section7[1].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname2}", "");
                        body = body.Replace("{cscompanynumber2}", "");
                        body = body.Replace("{cscompanydirector2}", "");
                        body = body.Replace("{csaddressline12}", "");
                        body = body.Replace("{csaddressline22}", "");
                        body = body.Replace("{csaddressline32}", "");
                        body = body.Replace("{cspostcode2}", "");
                        body = body.Replace("{cscountry2}", "");
                        body = body.Replace("{csnoofshare2}", "");
                    }

                    if (section7.Count >= 3)
                    {
                        body = body.Replace("{cscompanyname3}", section7[2].companyname != null ? section7[2].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber3}", section7[2].companyphonenumber != null ? section7[2].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector3}", section7[2].companydirector != null ? section7[2].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline13}", section7[2].registerofficeaddress != null ? section7[2].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline23}", section7[2].addressline2 != null ? section7[2].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline33}", section7[2].addressline3 != null ? section7[2].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode3}", section7[2].postalcode != null ? section7[2].postalcode.ToString() : "");
                        body = body.Replace("{cscountry3}", section7[2].country != null ? section7[2].country : "");
                        body = body.Replace("{csnoofshare3}", section7[2].numberofshare != null ? section7[2].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname3}", "");
                        body = body.Replace("{cscompanynumber3}", "");
                        body = body.Replace("{cscompanydirector3}", "");
                        body = body.Replace("{csaddressline13}", "");
                        body = body.Replace("{csaddressline23}", "");
                        body = body.Replace("{csaddressline33}", "");
                        body = body.Replace("{cspostcode3}", "");
                        body = body.Replace("{cscountry3}", "");
                        body = body.Replace("{csnoofshare3}", "");
                    }

                    if (section7.Count >= 4)
                    {
                        body = body.Replace("{cscompanyname4}", section7[3].companyname != null ? section7[3].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber4}", section7[3].companyphonenumber != null ? section7[3].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector4}", section7[3].companydirector != null ? section7[3].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline14}", section7[3].registerofficeaddress != null ? section7[3].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline24}", section7[3].addressline2 != null ? section7[3].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline34}", section7[3].addressline3 != null ? section7[3].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode4}", section7[3].postalcode != null ? section7[3].postalcode.ToString() : "");
                        body = body.Replace("{cscountry4}", section7[3].country != null ? section7[3].country : "");
                        body = body.Replace("{csnoofshare4}", section7[3].numberofshare != null ? section7[3].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname4}", "");
                        body = body.Replace("{cscompanynumber4}", "");
                        body = body.Replace("{cscompanydirector4}", "");
                        body = body.Replace("{csaddressline14}", "");
                        body = body.Replace("{csaddressline24}", "");
                        body = body.Replace("{csaddressline34}", "");
                        body = body.Replace("{cspostcode4}", "");
                        body = body.Replace("{cscountry4}", "");
                        body = body.Replace("{csnoofshare4}", "");
                    }

                    if (section7.Count >= 5)
                    {
                        body = body.Replace("{cscompanyname5}", section7[4].companyname != null ? section7[4].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber5}", section7[4].companyphonenumber != null ? section7[4].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector5}", section7[4].companydirector != null ? section7[4].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline15}", section7[4].registerofficeaddress != null ? section7[4].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline25}", section7[4].addressline2 != null ? section7[4].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline35}", section7[4].addressline3 != null ? section7[4].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode5}", section7[4].postalcode != null ? section7[4].postalcode.ToString() : "");
                        body = body.Replace("{cscountry5}", section7[4].country != null ? section7[4].country : "");
                        body = body.Replace("{csnoofshare5}", section7[4].numberofshare != null ? section7[4].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname5}", "");
                        body = body.Replace("{cscompanynumber5}", "");
                        body = body.Replace("{cscompanydirector5}", "");
                        body = body.Replace("{csaddressline15}", "");
                        body = body.Replace("{csaddressline25}", "");
                        body = body.Replace("{csaddressline35}", "");
                        body = body.Replace("{cspostcode5}", "");
                        body = body.Replace("{cscountry5}", "");
                        body = body.Replace("{csnoofshare5}", "");
                    }

                    if (section7.Count >= 6)
                    {
                        body = body.Replace("{cscompanyname6}", section7[5].companyname != null ? section7[5].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber6}", section7[5].companyphonenumber != null ? section7[5].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector6}", section7[5].companydirector != null ? section7[5].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline16}", section7[5].registerofficeaddress != null ? section7[5].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline26}", section7[5].addressline2 != null ? section7[5].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline36}", section7[5].addressline3 != null ? section7[5].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode6}", section7[5].postalcode != null ? section7[5].postalcode.ToString() : "");
                        body = body.Replace("{cscountry6}", section7[5].country != null ? section7[5].country : "");
                        body = body.Replace("{csnoofshare6}", section7[5].numberofshare != null ? section7[5].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname6}", "");
                        body = body.Replace("{cscompanynumber6}", "");
                        body = body.Replace("{cscompanydirector6}", "");
                        body = body.Replace("{csaddressline16}", "");
                        body = body.Replace("{csaddressline26}", "");
                        body = body.Replace("{csaddressline36}", "");
                        body = body.Replace("{cspostcode6}", "");
                        body = body.Replace("{cscountry6}", "");
                        body = body.Replace("{csnoofshare6}", "");
                    }

                    if (section7.Count >= 7)
                    {
                        body = body.Replace("{cscompanyname7}", section7[6].companyname != null ? section7[6].companyname.ToString() : "");
                        body = body.Replace("{cscompanynumber7}", section7[6].companyphonenumber != null ? section7[6].companyphonenumber.ToString() : "");
                        body = body.Replace("{cscompanydirector7}", section7[6].companydirector != null ? section7[6].companydirector.ToString() : "");
                        body = body.Replace("{csaddressline17}", section7[6].registerofficeaddress != null ? section7[6].registerofficeaddress.ToString() : "");
                        body = body.Replace("{csaddressline27}", section7[6].addressline2 != null ? section7[6].addressline2.ToString() : "");
                        body = body.Replace("{csaddressline37}", section7[6].addressline3 != null ? section7[6].addressline3.ToString() : "");
                        body = body.Replace("{cspostcode7}", section7[6].postalcode != null ? section7[6].postalcode.ToString() : "");
                        body = body.Replace("{cscountry7}", section7[6].country != null ? section7[6].country : "");
                        body = body.Replace("{csnoofshare7}", section7[6].numberofshare != null ? section7[6].numberofshare.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{cscompanyname7}", "");
                        body = body.Replace("{cscompanynumber7}", "");
                        body = body.Replace("{cscompanydirector7}", "");
                        body = body.Replace("{csaddressline17}", "");
                        body = body.Replace("{csaddressline27}", "");
                        body = body.Replace("{csaddressline37}", "");
                        body = body.Replace("{cspostcode7}", "");
                        body = body.Replace("{cscountry7}", "");
                        body = body.Replace("{csnoofshare7}", "");
                    }
                }

                var sectino8 = db.cls_beneficialowner_tbl.Where(x => x.cfid == cfid).ToList();
                if (sectino8 != null)
                {
                    if (sectino8.Count >= 1)
                    {
                        body = body.Replace("{boname1}", sectino8[0].name != null ? sectino8[0].name.ToString() : "");
                        body = body.Replace("{boaddressline11}", sectino8[0].addressline1 != null ? sectino8[0].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline21}", sectino8[0].addressline2 != null ? sectino8[0].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline31}", sectino8[0].addressline3 != null ? sectino8[0].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode1}", sectino8[0].postalcode != null ? sectino8[0].postalcode.ToString() : "");
                        body = body.Replace("{bocountry1}", sectino8[0].country != null ? sectino8[0].country.ToString() : "");
                        body = body.Replace("{bonationality1}", sectino8[0].nationality != null ? sectino8[0].nationality.ToString() : "");
                        body = body.Replace("{booccupation1}", sectino8[0].occupation != null ? sectino8[0].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo1}", sectino8[0].natureofownership != null ? sectino8[0].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname1}", "");
                        body = body.Replace("{boaddressline11}", "");
                        body = body.Replace("{boaddressline21}", "");
                        body = body.Replace("{boaddressline31}", "");
                        body = body.Replace("{bopostcode1}", "");
                        body = body.Replace("{bocountry1}", "");
                        body = body.Replace("{bonationality1}", "");
                        body = body.Replace("{booccupation1}", "");
                        body = body.Replace("{bonatureofbo1}", "");
                    }

                    if (sectino8.Count >= 2)
                    {
                        body = body.Replace("{boname2}", sectino8[1].name != null ? sectino8[1].name.ToString() : "");
                        body = body.Replace("{boaddressline12}", sectino8[1].addressline1 != null ? sectino8[1].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline22}", sectino8[1].addressline2 != null ? sectino8[1].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline32}", sectino8[1].addressline3 != null ? sectino8[1].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode2}", sectino8[1].postalcode != null ? sectino8[1].postalcode.ToString() : "");
                        body = body.Replace("{bocountry2}", sectino8[1].country != null ? sectino8[1].country.ToString() : "");
                        body = body.Replace("{bonationality2}", sectino8[1].nationality != null ? sectino8[1].nationality.ToString() : "");
                        body = body.Replace("{booccupation2}", sectino8[1].occupation != null ? sectino8[1].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo2}", sectino8[1].natureofownership != null ? sectino8[1].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname2}", "");
                        body = body.Replace("{boaddressline12}", "");
                        body = body.Replace("{boaddressline22}", "");
                        body = body.Replace("{boaddressline32}", "");
                        body = body.Replace("{bopostcode2}", "");
                        body = body.Replace("{bocountry2}", "");
                        body = body.Replace("{bonationality2}", "");
                        body = body.Replace("{booccupation2}", "");
                        body = body.Replace("{bonatureofbo2}", "");
                    }

                    if (sectino8.Count >= 3)
                    {
                        body = body.Replace("{boname3}", sectino8[2].name != null ? sectino8[2].name.ToString() : "");
                        body = body.Replace("{boaddressline13}", sectino8[2].addressline1 != null ? sectino8[2].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline23}", sectino8[2].addressline2 != null ? sectino8[2].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline33}", sectino8[2].addressline3 != null ? sectino8[2].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode3}", sectino8[2].postalcode != null ? sectino8[2].postalcode.ToString() : "");
                        body = body.Replace("{bocountry3}", sectino8[2].country != null ? sectino8[2].country.ToString() : "");
                        body = body.Replace("{bonationality3}", sectino8[2].nationality != null ? sectino8[2].nationality.ToString() : "");
                        body = body.Replace("{booccupation3}", sectino8[2].occupation != null ? sectino8[2].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo3}", sectino8[2].natureofownership != null ? sectino8[2].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname3}", "");
                        body = body.Replace("{boaddressline13}", "");
                        body = body.Replace("{boaddressline23}", "");
                        body = body.Replace("{boaddressline33}", "");
                        body = body.Replace("{bopostcode3}", "");
                        body = body.Replace("{bocountry3}", "");
                        body = body.Replace("{bonationality3}", "");
                        body = body.Replace("{booccupation3}", "");
                        body = body.Replace("{bonatureofbo3}", "");
                    }

                    if (sectino8.Count >= 4)
                    {
                        body = body.Replace("{boname4}", sectino8[3].name != null ? sectino8[3].name.ToString() : "");
                        body = body.Replace("{boaddressline14}", sectino8[3].addressline1 != null ? sectino8[3].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline24}", sectino8[3].addressline2 != null ? sectino8[3].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline34}", sectino8[3].addressline3 != null ? sectino8[3].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode4}", sectino8[3].postalcode != null ? sectino8[3].postalcode.ToString() : "");
                        body = body.Replace("{bocountry4}", sectino8[3].country != null ? sectino8[3].country.ToString() : "");
                        body = body.Replace("{bonationality4}", sectino8[3].nationality != null ? sectino8[3].nationality.ToString() : "");
                        body = body.Replace("{booccupation4}", sectino8[3].occupation != null ? sectino8[3].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo4}", sectino8[3].natureofownership != null ? sectino8[3].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname4}", "");
                        body = body.Replace("{boaddressline14}", "");
                        body = body.Replace("{boaddressline24}", "");
                        body = body.Replace("{boaddressline34}", "");
                        body = body.Replace("{bopostcode4}", "");
                        body = body.Replace("{bocountry4}", "");
                        body = body.Replace("{bonationality4}", "");
                        body = body.Replace("{booccupation4}", "");
                        body = body.Replace("{bonatureofbo4}", "");
                    }

                    if (sectino8.Count >= 5)
                    {
                        body = body.Replace("{boname5}", sectino8[4].name != null ? sectino8[4].name.ToString() : "");
                        body = body.Replace("{boaddressline15}", sectino8[4].addressline1 != null ? sectino8[4].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline25}", sectino8[4].addressline2 != null ? sectino8[4].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline35}", sectino8[4].addressline3 != null ? sectino8[4].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode5}", sectino8[4].postalcode != null ? sectino8[4].postalcode.ToString() : "");
                        body = body.Replace("{bocountry5}", sectino8[4].country != null ? sectino8[4].country.ToString() : "");
                        body = body.Replace("{bonationality5}", sectino8[4].nationality != null ? sectino8[4].nationality.ToString() : "");
                        body = body.Replace("{booccupation5}", sectino8[4].occupation != null ? sectino8[4].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo5}", sectino8[4].natureofownership != null ? sectino8[4].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname5}", "");
                        body = body.Replace("{boaddressline15}", "");
                        body = body.Replace("{boaddressline25}", "");
                        body = body.Replace("{boaddressline35}", "");
                        body = body.Replace("{bopostcode5}", "");
                        body = body.Replace("{bocountry5}", "");
                        body = body.Replace("{bonationality5}", "");
                        body = body.Replace("{booccupation5}", "");
                        body = body.Replace("{bonatureofbo5}", "");
                    }

                    if (sectino8.Count >= 6)
                    {
                        body = body.Replace("{boname6}", sectino8[5].name != null ? sectino8[5].name.ToString() : "");
                        body = body.Replace("{boaddressline16}", sectino8[5].addressline1 != null ? sectino8[5].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline26}", sectino8[5].addressline2 != null ? sectino8[5].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline36}", sectino8[5].addressline3 != null ? sectino8[5].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode6}", sectino8[5].postalcode != null ? sectino8[5].postalcode.ToString() : "");
                        body = body.Replace("{bocountry6}", sectino8[5].country != null ? sectino8[5].country.ToString() : "");
                        body = body.Replace("{bonationality6}", sectino8[5].nationality != null ? sectino8[5].nationality.ToString() : "");
                        body = body.Replace("{booccupation6}", sectino8[5].occupation != null ? sectino8[5].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo6}", sectino8[5].natureofownership != null ? sectino8[5].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname6}", "");
                        body = body.Replace("{boaddressline16}", "");
                        body = body.Replace("{boaddressline26}", "");
                        body = body.Replace("{boaddressline36}", "");
                        body = body.Replace("{bopostcode6}", "");
                        body = body.Replace("{bocountry6}", "");
                        body = body.Replace("{bonationality6}", "");
                        body = body.Replace("{booccupation6}", "");
                        body = body.Replace("{bonatureofbo6}", "");
                    }

                    if (sectino8.Count >= 7)
                    {
                        body = body.Replace("{boname7}", sectino8[6].name != null ? sectino8[6].name.ToString() : "");
                        body = body.Replace("{boaddressline17}", sectino8[6].addressline1 != null ? sectino8[6].addressline1.ToString() : "");
                        body = body.Replace("{boaddressline27}", sectino8[6].addressline2 != null ? sectino8[6].addressline2.ToString() : "");
                        body = body.Replace("{boaddressline37}", sectino8[6].addressline3 != null ? sectino8[6].addressline3.ToString() : "");
                        body = body.Replace("{bopostcode7}", sectino8[6].postalcode != null ? sectino8[6].postalcode.ToString() : "");
                        body = body.Replace("{bocountry7}", sectino8[6].country != null ? sectino8[6].country.ToString() : "");
                        body = body.Replace("{bonationality7}", sectino8[6].nationality != null ? sectino8[6].nationality.ToString() : "");
                        body = body.Replace("{booccupation7}", sectino8[6].occupation != null ? sectino8[6].occupation.ToString() : "");
                        body = body.Replace("{bonatureofbo7}", sectino8[6].natureofownership != null ? sectino8[6].natureofownership.ToString() : "");
                    }
                    else
                    {
                        body = body.Replace("{boname7}", "");
                        body = body.Replace("{boaddressline17}", "");
                        body = body.Replace("{boaddressline27}", "");
                        body = body.Replace("{boaddressline37}", "");
                        body = body.Replace("{bopostcode7}", "");
                        body = body.Replace("{bocountry7}", "");
                        body = body.Replace("{bonationality7}", "");
                        body = body.Replace("{booccupation7}", "");
                        body = body.Replace("{bonatureofbo7}", "");
                    }
                }

                var section9 = db.cls_addressdetails_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                if (section9 != null)
                {
                    body = body.Replace("{roaddressline1}", section9.roaddressline1 != null ? section9.roaddressline1.ToString() : "");
                    body = body.Replace("{roaddressline2}", section9.roaddressline2 != null ? section9.roaddressline2.ToString() : "");
                    body = body.Replace("{roaddressline3}", section9.oraddressline3 != null ? section9.oraddressline3.ToString() : "");
                    body = body.Replace("{ropostcode}", section9.ropostalcode != null ? section9.ropostalcode.ToString() : "");

                    body = body.Replace("{caaddressline1}", section9.caaddressline1 != null ? section9.caaddressline1.ToString() : "");
                    body = body.Replace("{caaddressline2}", section9.caaddressline2 != null ? section9.caaddressline2.ToString() : "");
                    body = body.Replace("{caaddressline3}", section9.caaddressline3 != null ? section9.caaddressline3.ToString() : "");
                    body = body.Replace("{capostcode}", section9.capostalcode != null ? section9.capostalcode.ToString() : "");
                    if (section9.roisalsothebusinessorcaaddress != null)
                    {
                        if (section9.roisalsothebusinessorcaaddress.ToString() == "true")
                            body = body.Replace("{roissameasca}", "Yes");
                        else
                            body = body.Replace("{roissameasca}", "No");
                    }
                    else
                    {
                        body = body.Replace("{roissameasca}", "No");
                    }
                }
                else
                {
                    body = body.Replace("{roaddressline1}", "");
                    body = body.Replace("{roaddressline2}", "");
                    body = body.Replace("{roaddressline3}", "");
                    body = body.Replace("{ropostcode}", "");

                    body = body.Replace("{caaddressline1}", "");
                    body = body.Replace("{caaddressline2}", "");
                    body = body.Replace("{caaddressline3}", "");
                    body = body.Replace("{capostcode}", "");
                    body = body.Replace("{roissameasca}", "");
                }

                var section10 = db.cls_additionalinfo_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                if (section10 != null)
                {
                    body = body.Replace("{additionalinfo}", section10.additionalinformation != null ? section10.additionalinformation.ToString() : "");
                }
                else
                {
                    body = body.Replace("{additionalinfo}", "");
                }
            }



            return body;
        }

        private string CreatePDFBody(decimal cfid)
        {

            string filePath = string.Empty;
            
            using (var db = new CompanyFormationdbEntities())
            {
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    Paragraph paragraph1blank = new Paragraph();
                    iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 10, 10, 10, 10);

                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    PdfPTable table;
                    PdfPCell cell;
                    iTextSharp.text.Paragraph paragraph;

                    PdfPTable table1 = new PdfPTable(2);
                    table1.SetWidthPercentage(new float[2] { 460f, 140f }, PageSize.LETTER);

                    #region section1
                    var section1 = db.cls_agree_tbl.Where(x => x.cfid == cfid).FirstOrDefault();

                    if (section1 != null)
                    {
                        //First section
                        #region agree
                        companyFolderName = section1.companyname != null ? section1.companyname.ToString() : "Temp";
                        var FontColour = new BaseColor(77, 24, 111);
                        var TIMES_ROMAN20 = FontFactory.GetFont("Calibri", 30, Font.BOLD, FontColour);

                        Paragraph paragraph1 = new Paragraph();
                        paragraph1.Add(new Chunk("CLS Online Company Order Form", TIMES_ROMAN20));
                        cell = new PdfPCell(paragraph1);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if (section1.agree >= 0)
                        {
                            var customeColor = new BaseColor(153, 51, 255);
                            //Paragraph paragraph2 = new Paragraph();
                            //paragraph2.Add(new Chunk("Agree to the CLS General Terms of Business have reviewed the CLS Handy Guide to Completing the Company Order Form:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk("We agree to the CLS General Terms of Business and have reviewed the CLS Handy Guide to Completing the Company Order Form:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section1.agree == 0 ? "Agree" : "DisAgree", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            //cell = new PdfPCell(paragraph2);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.PaddingRight = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Paragraph paragraph3 = new Paragraph();
                            paragraph3.Add(ch1);
                            paragraph3.Add(chspace);
                            paragraph3.Add(ch2);
                            cell = new PdfPCell(paragraph3);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                            table1.AddCell(cell);

                        }

                        if ((section1.nonthirdparties != null) && (section1.nonthirdparties.ToString() != ""))
                        {
                            //Paragraph paragraph4 = new Paragraph();
                            //paragraph4.Add(new Chunk("Anti Money Laundering Customer Due Diligence: ", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk c1 = new Chunk("Anti Money Laundering Customer Due Diligence: ", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk c2 = new Chunk(section1.nonthirdparties.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Chunk chspace = new Chunk("     ");
                            Paragraph paragraph5 = new Paragraph();
                            paragraph5.Add(c1);
                            paragraph5.Add(chspace);
                            paragraph5.Add(c2);
                            cell = new PdfPCell(paragraph5);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph5 = new Paragraph();
                            //paragraph5.Add(new Chunk(section1.nonthirdparties.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph5);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section1.incorporationtype != null) && (section1.incorporationtype.ToString() != ""))
                        {
                          //  Paragraph paragraph6 = new Paragraph();
                         //   paragraph6.Add(new Chunk("Incorporation Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk("Incorporation Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section1.incorporationtype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Chunk chspace = new Chunk("     ");
                            Paragraph paragraph7 = new Paragraph();
                            paragraph7.Add(ch1);
                            paragraph7.Add(chspace);
                            paragraph7.Add(ch2);

                            cell = new PdfPCell(paragraph7);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph7 = new Paragraph();
                            //paragraph7.Add(new Chunk(section1.incorporationtype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph7);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section1.companypacktype != null) && (section1.companypacktype.ToString() != ""))
                        {

                            Paragraph paragraph9 = new Paragraph();
                            paragraph9.Add(new Chunk(section1.companypacktype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk(section1.companypacktype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Chunk ch2 = new Chunk("Company Pack Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Paragraph paragraph8 = new Paragraph();
                            paragraph8.Add(ch2);
                            paragraph8.Add(chspace);
                            paragraph8.Add(ch1);
                            cell = new PdfPCell(paragraph8);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph9 = new Paragraph();
                            //paragraph9.Add(new Chunk(section1.companypacktype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph9);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section1.paymenttype != null) && (section1.paymenttype.ToString() != ""))
                        {
                          //  Paragraph paragraph11 = new Paragraph();
                          //  paragraph11.Add(new Chunk(section1.paymenttype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk(section1.paymenttype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Chunk ch2 = new Chunk("Payment Type (Select):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Paragraph paragraph10 = new Paragraph();
                            paragraph10.Add(ch2);
                            paragraph10.Add(chspace);
                            paragraph10.Add(ch1);
                            cell = new PdfPCell(paragraph10);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph11 = new Paragraph();
                            //paragraph11.Add(new Chunk(section1.paymenttype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph11);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        #endregion agree

                        #region Contact Details For Incorporation Purposes

                        #region blank
                       // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph12 = new Paragraph();
                        paragraph12.Add(new Chunk("Contact Details For Incorporation Purposes", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph12);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if ((section1.name != null) && (section1.name.ToString() != ""))
                        {
                            //Paragraph paragraph13 = new Paragraph();
                            //paragraph13.Add(new Chunk("Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph13);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph14 = new Paragraph();
                            //paragraph14.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph14);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //code to split name
                            string[] names = section1.name.Split(' ');



                            //Paragraph paragraph13firstname = new Paragraph();
                            //paragraph13firstname.Add(new Chunk("Firstname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph13firstname);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("First Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Chunk chspace = new Chunk("     ");
                            Paragraph paragraph14firstnamevalue = new Paragraph();
                            paragraph14firstnamevalue.Add(ch1);
                            paragraph14firstnamevalue.Add(chspace);
                            paragraph14firstnamevalue.Add(ch2);
                            cell = new PdfPCell(paragraph14firstnamevalue);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph13lastname = new Paragraph();
                            //paragraph13lastname.Add(new Chunk("Lastname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph13lastname);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch3 = new Chunk("Last Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch4 = new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph14lastnamevalue = new Paragraph();
                            paragraph14lastnamevalue.Add(ch3);
                            paragraph14lastnamevalue.Add(chspace);
                            paragraph14lastnamevalue.Add(ch4);
                            cell = new PdfPCell(paragraph14lastnamevalue);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);


                        }

                        if ((section1.companyname != null) && (section1.companyname.ToString() != ""))
                        {
                            //Paragraph paragraph15 = new Paragraph();
                            //paragraph15.Add(new Chunk("Practice/Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph15);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("Practice/Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section1.companyname.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Chunk chspace = new Chunk("     ");
                            Paragraph paragraph16 = new Paragraph();
                            paragraph16.Add(ch1);
                            paragraph16.Add(chspace);
                            paragraph16.Add(ch2);
                            cell = new PdfPCell(paragraph16);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            
                        }

                        if ((section1.addressline1 != null) && (section1.addressline1.ToString() != ""))
                        {
                            //Paragraph paragraph17 = new Paragraph();
                            //paragraph17.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph17);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section1.addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph18 = new Paragraph();
                            paragraph18.Add(ch1);
                            paragraph18.Add(chspace);
                            paragraph18.Add(ch2);
                            cell = new PdfPCell(paragraph18);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.addressline2 != null) && (section1.addressline2.ToString() != ""))
                        {
                            //Paragraph paragraph19 = new Paragraph();
                            //paragraph19.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph19);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section1.addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph20 = new Paragraph();
                            paragraph20.Add(ch1);
                            paragraph20.Add(chspace);
                            paragraph20.Add(ch2);
                            cell = new PdfPCell(paragraph20);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.addressline3 != null) && (section1.addressline3.ToString() != ""))
                        {
                            //Paragraph paragraph21 = new Paragraph();
                            //paragraph21.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph21);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(new Chunk(section1.addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE)));

                            Paragraph paragraph22 = new Paragraph();
                            paragraph22.Add(ch1);
                            paragraph22.Add(chspace);
                            paragraph22.Add(ch2);
                            cell = new PdfPCell(paragraph22);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.postcode != null) && (section1.postcode.ToString() != ""))
                        {
                            //Paragraph paragraph23 = new Paragraph();
                            //paragraph23.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph23);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section1.postcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph24 = new Paragraph();
                            paragraph24.Add(ch1);
                            paragraph24.Add(chspace);
                            paragraph24.Add(ch2);
                            cell = new PdfPCell(paragraph24);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.phonenumber != null) && (section1.phonenumber.ToString() != ""))
                        {
                            //Paragraph paragraph25 = new Paragraph();
                            //paragraph25.Add(new Chunk("Phone Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph25);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("Phone Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section1.phonenumber.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph26 = new Paragraph();
                            paragraph26.Add(ch1);
                            paragraph26.Add(chspace);
                            paragraph26.Add(ch2);
                            cell = new PdfPCell(paragraph26);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.email != null) && (section1.email.ToString() != ""))
                        {
                            //Paragraph paragraph27 = new Paragraph();
                            //paragraph27.Add(new Chunk("E-Mail Address:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph27);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch1 = new Chunk("E-Mail Address:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section1.email.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph28 = new Paragraph();
                            paragraph28.Add(ch1);
                            paragraph28.Add(chspace);
                            paragraph28.Add(ch2);

                            cell = new PdfPCell(paragraph28);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;

                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        #endregion Contact Details For Incorporation Purposes
                       
                    }
                    #endregion section1

                    #region section2
                    var section2 = db.cls_companyincorporation_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section2 != null)
                    {
                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        #region Company Incorporation Required Details
                        Paragraph paragraph29 = new Paragraph();
                        paragraph29.Add(new Chunk("Company Incorporation Required Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph29);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph30 = new Paragraph();
                        paragraph30.Add(new Chunk("Proposed Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph30);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        if ((section2.firstchoice != null) && (section2.firstchoice.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("First Choice:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section2.firstchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));


                            Paragraph paragraph31 = new Paragraph();
                            paragraph31.Add(ch1);
                            paragraph31.Add(chspace);
                            paragraph31.Add(ch2);
                            cell = new PdfPCell(paragraph31);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph32 = new Paragraph();
                            //paragraph32.Add(new Chunk("First Choice", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph32);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph33 = new Paragraph();
                            //paragraph33.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph33);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph34 = new Paragraph();
                            //paragraph34.Add(new Chunk(section2.firstchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph34);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        if ((section2.secondchoice != null) && (section2.secondchoice.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Second Choice:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section2.secondchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph35 = new Paragraph();
                            paragraph35.Add(ch1);
                            paragraph35.Add(chspace);
                            paragraph35.Add(ch2);
                            cell = new PdfPCell(paragraph35);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph36 = new Paragraph();
                            //paragraph36.Add(new Chunk("Second Choice", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph36);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph37 = new Paragraph();
                            //paragraph37.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph37);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph38 = new Paragraph();
                            //paragraph38.Add(new Chunk(section2.secondchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph38);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        if ((section2.thirdchoice != null) && (section2.thirdchoice.ToString() != ""))
                        {

                            Chunk ch1 = new Chunk("Third Choice:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section2.thirdchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph39 = new Paragraph();
                            paragraph39.Add(ch1);
                            paragraph39.Add(chspace);
                            paragraph39.Add(ch2);
                            cell = new PdfPCell(paragraph39);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph40 = new Paragraph();
                            //paragraph40.Add(new Chunk("Third Choice", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph40);
                            //cell.BorderWidth = 0; 
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph41 = new Paragraph();
                            //paragraph41.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph41);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph42 = new Paragraph();
                            //paragraph42.Add(new Chunk(section2.thirdchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph42);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        if ((section2.principalactivity != null) && (section2.principalactivity.ToString() != ""))
                        {

                            Paragraph paragraph44 = new Paragraph();
                            paragraph44.Add(new Chunk("Principal Activity of the Company:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph44);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; 
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Chunk ch1 = new Chunk("Principal Activity:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section2.principalactivity.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph45 = new Paragraph();
                            paragraph45.Add(ch1);
                            paragraph45.Add(chspace);
                            paragraph45.Add(ch2);
                            cell = new PdfPCell(paragraph45);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph46 = new Paragraph();
                            //paragraph46.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph46);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph47 = new Paragraph();
                            //paragraph47.Add(new Chunk(section2.principalactivity.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph47);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            if ((section2.additionalwording != null) && (section2.additionalwording.ToString() != ""))
                            {
                                Chunk ch3 = new Chunk("Additional Wording:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                
                                Chunk ch4 = new Chunk(section2.additionalwording.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                Paragraph paragraph44additionalwording = new Paragraph();
                                paragraph44additionalwording.Add(ch3);
                                paragraph44additionalwording.Add(chspace);
                                paragraph44additionalwording.Add(ch4);
                                cell = new PdfPCell(paragraph44additionalwording);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //Paragraph paragraph45additionalwording = new Paragraph();
                                //paragraph45additionalwording.Add(new Chunk("Additional Wording", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                //cell = new PdfPCell(paragraph45additionalwording);
                                //cell.BorderWidth = 0;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);

                                //Paragraph paragraph46additionalwording = new Paragraph();
                                //paragraph46additionalwording.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                //cell = new PdfPCell(paragraph46additionalwording);
                                //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);

                                //Paragraph paragraph47additionalwording = new Paragraph();
                                //paragraph47additionalwording.Add(new Chunk(section2.additionalwording.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                //cell = new PdfPCell(paragraph47additionalwording);
                                //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                            }


                        }
                        if ((section2.companytype != null) && (section2.companytype.ToString() != ""))
                        {

                            Chunk ch1 = new Chunk("Company Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk chspace = new Chunk("     ");
                            Chunk ch2 = new Chunk(section2.companytype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Paragraph paragraph48 = new Paragraph();
                            paragraph48.Add(ch1);
                            paragraph48.Add(chspace);
                            paragraph48.Add(ch2);

                            cell = new PdfPCell(paragraph48);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph49 = new Paragraph();
                            //paragraph49.Add(new Chunk(section2.companytype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph49);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        #endregion Company Incorporation Required Details
                    }
                    #endregion section2

                    #region section3
                    #region Share Capital
                    var section3 = db.cls_sharecapital_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section3 != null)
                    {
                        Chunk chspace = new Chunk("     ");

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 5, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                       // cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph50 = new Paragraph();
                        paragraph50.Add(new Chunk("Share Capital", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph50);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0; 
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        //#region blank
                        ////Paragraph paragraph1blank = new Paragraph();
                        //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        //cell = new PdfPCell(paragraph1blank);
                        //cell.Colspan = 2;
                        //cell.BorderWidth = 0;
                        //cell.Padding = 0;
                        //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        //table1.AddCell(cell);
                        //#endregion blank

                        if ((section3.issuedsharecapital != null) && (section3.issuedsharecapital.ToString()!=""))
                        {
                            Chunk ch1 = new Chunk("Issued Share Capital:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section3.issuedsharecapital.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph51 = new Paragraph();
                            paragraph51.Add(ch1);
                            paragraph51.Add(chspace);
                            paragraph51.Add(ch2);

                            cell = new PdfPCell(paragraph51);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph52 = new Paragraph();
                            //paragraph52.Add(new Chunk(section3.issuedsharecapital.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph52);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section3.nominalamoutpershare != null) && (section3.nominalamoutpershare.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Nominal Amount Per Share:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section3.nominalamoutpershare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph53 = new Paragraph();
                            paragraph53.Add(ch1);
                            paragraph53.Add(chspace);
                            paragraph53.Add(ch2);

                            cell = new PdfPCell(paragraph53);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph54 = new Paragraph();
                            //paragraph54.Add();
                            //cell = new PdfPCell(paragraph54);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section3.shareclass != null) && (section3.shareclass.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Share Class:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section3.shareclass.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Paragraph paragraph55 = new Paragraph();
                            paragraph55.Add(ch1);
                            paragraph55.Add(chspace);
                            paragraph55.Add(ch2);
                            cell = new PdfPCell(paragraph55);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph56 = new Paragraph();
                            //paragraph56.Add();
                            //cell = new PdfPCell(paragraph56);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section3.authorisedsharecapital != null) && (section3.authorisedsharecapital.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Authorised Share Capital (Optional for LTD):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section3.authorisedsharecapital.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph57 = new Paragraph();
                            paragraph57.Add(ch1);
                            paragraph57.Add(chspace);
                            paragraph57.Add(ch2);
                            cell = new PdfPCell(paragraph57);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph58 = new Paragraph();
                            //paragraph58.Add();
                            //cell = new PdfPCell(paragraph58);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                    }
                    #endregion Share Capital
                    #endregion section3

                    #region section4

                    var section4 = db.cls_secretary_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section4 != null)
                    {
                        #region Company Secretary Details

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph59 = new Paragraph();
                        paragraph59.Add(new Chunk("Company Secretary Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph59);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph60 = new Paragraph();
                        paragraph60.Add(new Chunk("Individual Secretary Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph60);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        Chunk chspace = new Chunk("     ");
                        if ((section4.name != null) && (section4.name.ToString() != ""))
                        {
                            string[] names = section4.name.Split(' ');

                            Chunk ch1 = new Chunk("First Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph61 = new Paragraph();
                            paragraph61.Add(ch1);
                            paragraph61.Add(chspace);
                            paragraph61.Add(ch2);
                            cell = new PdfPCell(paragraph61);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph62 = new Paragraph();
                            //paragraph62.Add();
                            //cell = new PdfPCell(paragraph62);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Chunk ch3 = new Chunk("Last Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch4 = new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Paragraph paragraph61lastname = new Paragraph();
                            paragraph61lastname.Add(ch3);
                            paragraph61lastname.Add(chspace);
                            paragraph61lastname.Add(ch4);
                            cell = new PdfPCell(paragraph61lastname);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph62lastname = new Paragraph();
                            //paragraph62lastname.Add();
                            //cell = new PdfPCell(paragraph62lastname);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                        }

                        if ((section4.dob != null) && (section4.dob.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Date of Birth:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.dob != null ? Convert.ToDateTime(section4.dob).ToString("dd-MM-yyyy").Replace('-', '/') : "", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph63 = new Paragraph();
                            paragraph63.Add(ch1);
                            paragraph63.Add(chspace);
                            paragraph63.Add(ch2);
                            cell = new PdfPCell(paragraph63);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph64 = new Paragraph();
                            //paragraph64.Add();
                            //cell = new PdfPCell(paragraph64);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.addressline1 != null) && (section4.addressline1.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Residential Address 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            //Chunk ch1 = new Chunk("Residential Address 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph65 = new Paragraph();
                            paragraph65.Add(ch1);
                            paragraph65.Add(chspace);
                            paragraph65.Add(ch2);

                            cell = new PdfPCell(paragraph65);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph66 = new Paragraph();
                            //paragraph66.Add();
                            //cell = new PdfPCell(paragraph66);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.addressline2 != null) && (section4.addressline2.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Residential Address 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph67 = new Paragraph();
                            paragraph67.Add(ch1);
                            paragraph67.Add(chspace);
                            paragraph67.Add(ch2); 
                            cell = new PdfPCell(paragraph67);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph68 = new Paragraph();
                            //paragraph68.Add();
                            //cell = new PdfPCell(paragraph68);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.addressline3 != null) && (section4.addressline3.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Residential Address 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Paragraph paragraph69 = new Paragraph();
                            paragraph69.Add(ch1);
                            paragraph69.Add(chspace);
                            paragraph69.Add(ch2);
                            cell = new PdfPCell(paragraph69);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph70 = new Paragraph();
                            //paragraph70.Add();
                            //cell = new PdfPCell(paragraph70);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.postalcode != null) && (section4.postalcode.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph71 = new Paragraph();
                            paragraph71.Add(ch1);
                            paragraph71.Add(chspace);
                            paragraph71.Add(ch2);
                            cell = new PdfPCell(paragraph71);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph72 = new Paragraph();
                            //paragraph72.Add();
                            //cell = new PdfPCell(paragraph72);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.country != null) && (section4.country.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph73 = new Paragraph();
                            paragraph73.Add(ch1);
                            paragraph73.Add(chspace);
                            paragraph73.Add(ch2);
                            cell = new PdfPCell(paragraph73);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph74 = new Paragraph();
                            //paragraph74.Add();
                            //cell = new PdfPCell(paragraph74);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.companyname != null) && (section4.companyname.ToString() != ""))
                        {

                            #region blank
                            //Paragraph paragraph1blank = new Paragraph();
                            paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            cell = new PdfPCell(paragraph1blank);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            #endregion blank

                            Paragraph paragraph75 = new Paragraph();
                            paragraph75.Add(new Chunk("Corporate Secretary Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph75);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; 
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);


                            #region blank
                            //Paragraph paragraph1blank = new Paragraph();
                            paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            cell = new PdfPCell(paragraph1blank);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            #endregion blank

                            Chunk ch1 = new Chunk("Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.companyname.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph76 = new Paragraph();
                            paragraph76.Add(ch1);
                            paragraph76.Add(chspace);
                            paragraph76.Add(ch2);
                            cell = new PdfPCell(paragraph76);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph77 = new Paragraph();
                            //paragraph77.Add();
                            //cell = new PdfPCell(paragraph77);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.companynumber != null) && (section4.companynumber.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.companynumber.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph78 = new Paragraph();
                            paragraph78.Add(ch1);
                            paragraph78.Add(chspace);
                            paragraph78.Add(ch2);
                            cell = new PdfPCell(paragraph78);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph79 = new Paragraph();
                            //paragraph79.Add();
                            //cell = new PdfPCell(paragraph79);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.companydirector != null) && (section4.companydirector.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Company Director (signing on behalf of the Company):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.companydirector.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph80 = new Paragraph();
                            paragraph80.Add(ch1);
                            paragraph80.Add(chspace);
                            paragraph80.Add(ch2);

                            cell = new PdfPCell(paragraph80);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph81 = new Paragraph();
                            //paragraph81.Add();
                            //cell = new PdfPCell(paragraph81);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.companyregionaloffice != null) && (section4.companyregionaloffice.ToString() != ""))
                        {
                            //Chunk ch1 = new Chunk("Residential Address 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch1 = new Chunk("Registered Office:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.companyregionaloffice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph82 = new Paragraph();
                            paragraph82.Add(ch1);
                            paragraph82.Add(chspace);
                            paragraph82.Add(ch2);
                            cell = new PdfPCell(paragraph82);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph83 = new Paragraph();
                            //paragraph83.Add();
                            //cell = new PdfPCell(paragraph83);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.companyaddressline1 != null) && (section4.companyaddressline1.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Residential Address 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.companyaddressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph84 = new Paragraph();
                            paragraph84.Add(ch1);
                            paragraph84.Add(chspace);
                            paragraph84.Add(ch2);
                            cell = new PdfPCell(paragraph84);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph85 = new Paragraph();
                            //paragraph85.Add();
                            //cell = new PdfPCell(paragraph85);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.companyaddressline2 != null) && (section4.companyaddressline2.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Residential Address 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.companyaddressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph86 = new Paragraph();
                            paragraph86.Add(ch1);
                            paragraph86.Add(chspace);
                            paragraph86.Add(ch2);
                            cell = new PdfPCell(paragraph86);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph87 = new Paragraph();
                            //paragraph87.Add();
                            //cell = new PdfPCell(paragraph87);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.compnaypostal != null) && (section4.compnaypostal.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.compnaypostal.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph88 = new Paragraph();
                            paragraph88.Add(ch1);
                            paragraph88.Add(chspace);
                            paragraph88.Add(ch2);
                            cell = new PdfPCell(paragraph88);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph89 = new Paragraph();
                            //paragraph89.Add();
                            //cell = new PdfPCell(paragraph89);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section4.compnaycountry != null) && (section4.compnaycountry.ToString() != ""))
                        {
                            Chunk ch1 = new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section4.compnaycountry.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph90 = new Paragraph();
                            paragraph90.Add(ch1);
                            paragraph90.Add(chspace);
                            paragraph90.Add(ch2);
                            cell = new PdfPCell(paragraph90);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph91 = new Paragraph();
                            //paragraph91.Add();
                            //cell = new PdfPCell(paragraph91);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        #endregion Company Secretary Details
                    }
                    #endregion section4

                    #region section5
                    var section5 = db.cls_director_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section5 != null)
                    {
                        #region director


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph92 = new Paragraph();
                        paragraph92.Add(new Chunk("Company Director(s)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph92);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        if (section5.Count > 0)
                        {
                            Chunk chspace = new Chunk("     ");
                            for (int i = 0; i < section5.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                                if (i==3)
                                {

                                    

                                    Paragraph paragraph92addtional = new Paragraph();
                                    paragraph92addtional.Add(new Chunk("Additional Director Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph92addtional);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; 
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);


                                    
                                }
                                #region director 1
                                //director 1
                                int n = 1 + i;

                                Paragraph paragraph93 = new Paragraph();
                                paragraph93.Add(new Chunk("Director "+ n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph93);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //#region blank
                                ////Paragraph paragraph1blank = new Paragraph();
                                //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                //cell = new PdfPCell(paragraph1blank);
                                //cell.Colspan = 2;
                                //cell.BorderWidth = 0;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                                //#endregion blank
                                if ((section5[i].name != null) && (section5[i].name.ToString() != ""))
                                {
                                    string[] names = section5[i].name.Split(' ');

                                    Chunk ch1 = new Chunk("First Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph94 = new Paragraph();
                                    paragraph94.Add(ch1);
                                    paragraph94.Add(chspace);
                                    paragraph94.Add(ch2);
                                    cell = new PdfPCell(paragraph94);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph95 = new Paragraph();
                                    //paragraph95.Add();
                                    //cell = new PdfPCell(paragraph95);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);

                                    Chunk ch3 = new Chunk("Last Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch4 = new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph94lastname = new Paragraph();
                                    paragraph94lastname.Add(ch3);
                                    paragraph94lastname.Add(chspace);
                                    paragraph94lastname.Add(ch4);
                                    cell = new PdfPCell(paragraph94lastname);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph95lastname = new Paragraph();
                                    //paragraph95lastname.Add();
                                    //cell = new PdfPCell(paragraph95lastname);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].dob != null) && (section5[i].dob.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Date of Birth:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].dob != null ? Convert.ToDateTime(section5[i].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph96 = new Paragraph();
                                    paragraph96.Add(ch1);
                                    paragraph96.Add(chspace);
                                    paragraph96.Add(ch2);
                                    cell = new PdfPCell(paragraph96);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph97 = new Paragraph();
                                    //paragraph97.Add();
                                    //cell = new PdfPCell(paragraph97);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].occupation != null) && (section5[i].occupation.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Occupation:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].occupation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph98 = new Paragraph();
                                    paragraph98.Add(ch1);
                                    paragraph98.Add(chspace);
                                    paragraph98.Add(ch2);
                                    cell = new PdfPCell(paragraph98);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph99 = new Paragraph();
                                    //paragraph99.Add();
                                    //cell = new PdfPCell(paragraph99);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].addressline1 != null) && (section5[i].addressline1.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Residential Address 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph100 = new Paragraph();
                                    paragraph100.Add(ch1);
                                    paragraph100.Add(chspace);
                                    paragraph100.Add(ch2);

                                    cell = new PdfPCell(paragraph100);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph101 = new Paragraph();
                                    //paragraph101.Add();
                                    //cell = new PdfPCell(paragraph101);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].addressline2 != null) && (section5[i].addressline2.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Residential Address 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph102 = new Paragraph();
                                    paragraph102.Add(ch1);
                                    paragraph102.Add(chspace);
                                    paragraph102.Add(ch2);
                                    cell = new PdfPCell(paragraph102);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph103 = new Paragraph();
                                    //paragraph103.Add();
                                    //cell = new PdfPCell(paragraph103);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].addressline3 != null) && (section5[i].addressline3.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Residential Address 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph104 = new Paragraph();
                                    paragraph104.Add(ch1);
                                    paragraph104.Add(chspace);
                                    paragraph104.Add(ch2);
                                    cell = new PdfPCell(paragraph104);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph105 = new Paragraph();
                                    //paragraph105.Add();
                                    //cell = new PdfPCell(paragraph105);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].postalcode != null) && (section5[i].postalcode.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph106 = new Paragraph();
                                    paragraph106.Add(ch1);
                                    paragraph106.Add(chspace);
                                    paragraph106.Add(ch2);
                                    cell = new PdfPCell(paragraph106);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph107 = new Paragraph();
                                    //paragraph107.Add();
                                    //cell = new PdfPCell(paragraph107);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].country != null) && (section5[i].country.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph108 = new Paragraph();
                                    paragraph108.Add(ch1);
                                    paragraph108.Add(chspace);
                                    paragraph108.Add(ch2);
                                    cell = new PdfPCell(paragraph108);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph109 = new Paragraph();
                                    //paragraph109.Add();
                                    //cell = new PdfPCell(paragraph109);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].nationality != null) && (section5[i].nationality.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Nationality:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].nationality.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph110 = new Paragraph();
                                    paragraph110.Add(ch1);
                                    paragraph110.Add(chspace);
                                    paragraph110.Add(ch2);
                                    cell = new PdfPCell(paragraph110);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph111 = new Paragraph();
                                    //paragraph111.Add();
                                    //cell = new PdfPCell(paragraph111);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].otherdirectorship1 != null) && (section5[i].otherdirectorship1.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Other Directorship 1 - Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].otherdirectorship1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph112 = new Paragraph();
                                    paragraph112.Add(ch1);
                                    paragraph112.Add(chspace);
                                    paragraph112.Add(ch2);
                                    cell = new PdfPCell(paragraph112);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph113 = new Paragraph();
                                    //paragraph113.Add();
                                    //cell = new PdfPCell(paragraph113);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].otherdirectorship2 != null) && (section5[i].otherdirectorship2.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Other Directorship 2 - Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].otherdirectorship2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph114 = new Paragraph();
                                    paragraph114.Add(ch1);
                                    paragraph114.Add(chspace);
                                    paragraph114.Add(ch2);
                                    cell = new PdfPCell(paragraph114);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph115 = new Paragraph();
                                    //paragraph115.Add();
                                    //cell = new PdfPCell(paragraph115);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].otherdirectorship3 != null) && (section5[i].otherdirectorship3.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Other Directorship 3 - Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].otherdirectorship3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph116 = new Paragraph();
                                    paragraph116.Add(ch1);
                                    paragraph116.Add(chspace);
                                    paragraph116.Add(ch2);
                                    cell = new PdfPCell(paragraph116);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph117 = new Paragraph();
                                    //paragraph117.Add();
                                    //cell = new PdfPCell(paragraph117);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].restricted != null) && (section5[i].restricted.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Disqualified or Restricted:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].restricted.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph118 = new Paragraph();
                                    paragraph118.Add(ch1);
                                    paragraph118.Add(chspace);
                                    paragraph118.Add(ch2);
                                    cell = new PdfPCell(paragraph118);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph119 = new Paragraph();
                                    //paragraph119.Add();
                                    //cell = new PdfPCell(paragraph119);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].numberofshare != null) && (section5[i].numberofshare.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("If this director is also a subscriber, enter their number of shares:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].numberofshare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph120 = new Paragraph();
                                    paragraph120.Add(ch1);
                                    paragraph120.Add(chspace);
                                    paragraph120.Add(ch2);
                                    cell = new PdfPCell(paragraph120);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph121 = new Paragraph();
                                    //paragraph121.Add();
                                    //cell = new PdfPCell(paragraph121);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section5[i].beneficialowner != null) && (section5[i].beneficialowner.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Is the director the beneficial owner of the above shares?:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section5[i].beneficialowner.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph122 = new Paragraph();
                                    paragraph122.Add(ch1);
                                    paragraph122.Add(chspace);
                                    paragraph122.Add(ch2);
                                    cell = new PdfPCell(paragraph122);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph123 = new Paragraph();
                                    //paragraph123.Add();
                                    //cell = new PdfPCell(paragraph123);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }
                                #endregion director 1
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                            }

                        }

                        #endregion director
                    }
                    #endregion section5

                    #region section6
                    var section6 = db.cls_subscriber_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section6 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        if (section6.Count > 0)
                        {
                            #region Subscriber Details (Individual)
                            Paragraph paragraph311 = new Paragraph();
                        paragraph311.Add(new Chunk("Subscriber Details (Individual)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph311);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                            //#region blank
                            //Paragraph paragraph1blank = new Paragraph();
                            //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            //cell = new PdfPCell(paragraph1blank);
                            //cell.Colspan = 2;
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //#endregion blank

                            Chunk chspace = new Chunk("     ");
                            for (int i = 0; i < section6.Count; i++)
                            {

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                                if (i == 3)
                                {
                                    Paragraph paragraph311additional = new Paragraph();
                                    paragraph311additional.Add(new Chunk("Additional Subscriber Details (Individual)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph311additional);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }
                                #region  Subscriber 1
                                //director 7
                                int n = 1 + i;
                                Paragraph paragraph312 = new Paragraph();
                                paragraph312.Add(new Chunk("Subscriber "+n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph312);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                if ((section6[i].name != null) && (section6[i].name.ToString() != null))
                                {
                                    string[] names = section6[i].name.Split(' ');

                                    Chunk ch1 = new Chunk("First Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph313 = new Paragraph();
                                    paragraph313.Add(ch1);
                                    paragraph313.Add(chspace);
                                    paragraph313.Add(ch2);
                                    cell = new PdfPCell(paragraph313);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph314 = new Paragraph();
                                    //paragraph314.Add();
                                    //cell = new PdfPCell(paragraph314);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);

                                    Chunk ch3 = new Chunk("Last Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch4 = new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph313lastname = new Paragraph();
                                    paragraph313lastname.Add(ch3);
                                    paragraph313lastname.Add(chspace);
                                    paragraph313lastname.Add(ch4);

                                    cell = new PdfPCell(paragraph313lastname);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph314lastname = new Paragraph();
                                    //paragraph314lastname.Add();
                                    //cell = new PdfPCell(paragraph314lastname);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);

                                }

                                if ((section6[i].addressline1 != null) && (section6[i].addressline1.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph315 = new Paragraph();
                                    paragraph315.Add(ch1);
                                    paragraph315.Add(chspace);
                                    paragraph315.Add(ch2);
                                    cell = new PdfPCell(paragraph315);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph316 = new Paragraph();
                                    //paragraph316.Add();
                                    //cell = new PdfPCell(paragraph316);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].addressline2 != null) && (section6[i].addressline2.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph317 = new Paragraph();
                                    paragraph317.Add(ch1);
                                    paragraph317.Add(chspace);
                                    paragraph317.Add(ch2);
                                    cell = new PdfPCell(paragraph317);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph318 = new Paragraph();
                                    //paragraph318.Add();
                                    //cell = new PdfPCell(paragraph318);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].addressline3 != null) && (section6[i].addressline3.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph319 = new Paragraph();
                                    paragraph319.Add(ch1);
                                    paragraph319.Add(chspace);
                                    paragraph319.Add(ch2);
                                    cell = new PdfPCell(paragraph319);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph320 = new Paragraph();
                                    //paragraph320.Add();
                                    //cell = new PdfPCell(paragraph320);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].postalcode != null) && (section6[i].postalcode.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph321 = new Paragraph();
                                    paragraph321.Add(ch1);
                                    paragraph321.Add(chspace);
                                    paragraph321.Add(ch2);
                                    cell = new PdfPCell(paragraph321);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph322 = new Paragraph();
                                    //paragraph322.Add();
                                    //cell = new PdfPCell(paragraph322);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].country != null) && (section6[i].country.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph323 = new Paragraph();
                                    paragraph323.Add(ch1);
                                    paragraph323.Add(chspace);
                                    paragraph323.Add(ch2);
                                    cell = new PdfPCell(paragraph323);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph324 = new Paragraph();
                                    //paragraph324.Add();
                                    //cell = new PdfPCell(paragraph324);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].nationality != null) && (section6[i].nationality.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Nationality:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].nationality.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph325 = new Paragraph();
                                    paragraph325.Add(ch1);
                                    paragraph325.Add(chspace);
                                    paragraph325.Add(ch2);
                                    cell = new PdfPCell(paragraph325);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph326 = new Paragraph();
                                    //paragraph326.Add();
                                    //cell = new PdfPCell(paragraph326);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].occupation != null) && (section6[i].occupation.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Occupation:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].occupation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph327 = new Paragraph();
                                    paragraph327.Add(ch1);
                                    paragraph327.Add(chspace);
                                    paragraph327.Add(ch2);
                                    cell = new PdfPCell(paragraph327);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph328 = new Paragraph();
                                    //paragraph328.Add();
                                    //cell = new PdfPCell(paragraph328);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].numberofshare != null) && (section6[i].numberofshare.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Number of Shares:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].numberofshare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph329 = new Paragraph();
                                    paragraph329.Add(ch1);
                                    paragraph329.Add(chspace);
                                    paragraph329.Add(ch2);
                                    cell = new PdfPCell(paragraph329);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph330 = new Paragraph();
                                    //paragraph330.Add();
                                    //cell = new PdfPCell(paragraph330);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section6[i].beneficialowner != null) && (section6[i].beneficialowner.ToString() != null))
                                {
                                    Chunk ch1 = new Chunk("Is the subscriber the beneficial owner of the shares?:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section6[i].beneficialowner.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph331 = new Paragraph();
                                    paragraph331.Add(ch1);
                                    paragraph331.Add(chspace);
                                    paragraph331.Add(ch2);
                                    cell = new PdfPCell(paragraph331);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph332 = new Paragraph();
                                    //paragraph332.Add();
                                    //cell = new PdfPCell(paragraph332);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }
                                #endregion Subscriber 1

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                            }
                            #endregion Subscriber Details (Individual)
                        }

                    }
                    #endregion section6

                    #region section7
                    var section7 = db.cls_corporatesubscriber_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section7 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        if (section7.Count > 0)
                        {
                            #region Corporate Subscriber
                            Paragraph paragraph1101 = new Paragraph();
                            paragraph1101.Add(new Chunk("Corporate Subscriber Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                            cell = new PdfPCell(paragraph1101);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //#region blank
                            ////Paragraph paragraph1blank = new Paragraph();
                            //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            //cell = new PdfPCell(paragraph1blank);
                            //cell.Colspan = 2;
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //#endregion blank
                            Chunk chspace = new Chunk("     ");
                            for (int i = 0; i < section7.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                                if (i == 3)
                                {
                                    Paragraph paragraph1101addtionalcorporatesubscriber = new Paragraph();
                                    paragraph1101addtionalcorporatesubscriber.Add(new Chunk("Additional Corporate Subscriber Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph1101addtionalcorporatesubscriber);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                #region  CorporateSubscriber 1
                                int n = 1 + i;
                                Paragraph paragraph1102 = new Paragraph();
                                paragraph1102.Add(new Chunk("Corporate Subscriber " + n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph1102);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                if ((section7[i].companyname != null) && (section7[i].companyname.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].companyname.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph1103 = new Paragraph();
                                    paragraph1103.Add(ch1);
                                    paragraph1103.Add(chspace);
                                    paragraph1103.Add(ch2);
                                    cell = new PdfPCell(paragraph1103);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1104 = new Paragraph();
                                    //paragraph1104.Add();
                                    //cell = new PdfPCell(paragraph1104);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);

                                }

                                if ((section7[i].companyphonenumber != null) && (section7[i].companyphonenumber.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].companyphonenumber.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph1105 = new Paragraph();
                                    paragraph1105.Add(ch1);
                                    paragraph1105.Add(chspace);
                                    paragraph1105.Add(ch2);
                                    cell = new PdfPCell(paragraph1105);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1106 = new Paragraph();
                                    //paragraph1106.Add();
                                    //cell = new PdfPCell(paragraph1106);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].companydirector != null) && (section7[i].companydirector.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Company Director (Signing):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].companydirector.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph1107 = new Paragraph();
                                    paragraph1107.Add(ch1);
                                    paragraph1107.Add(chspace);
                                    paragraph1107.Add(ch2);

                                    cell = new PdfPCell(paragraph1107);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1108 = new Paragraph();
                                    //paragraph1108.Add();
                                    //cell = new PdfPCell(paragraph1108);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].registerofficeaddress != null) && (section7[i].registerofficeaddress.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Registered Office Address:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].registerofficeaddress.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph1109 = new Paragraph();
                                    paragraph1109.Add(ch1);
                                    paragraph1109.Add(chspace);
                                    paragraph1109.Add(ch2);
                                    cell = new PdfPCell(paragraph1109);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1110 = new Paragraph();
                                    //paragraph1110.Add();
                                    //cell = new PdfPCell(paragraph1110);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].addressline2 != null) && (section7[i].addressline2.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph1111 = new Paragraph();
                                    paragraph1111.Add(ch1);
                                    paragraph1111.Add(chspace);
                                    paragraph1111.Add(ch2);
                                    cell = new PdfPCell(paragraph1111);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1112 = new Paragraph();
                                    //paragraph1112.Add();
                                    //cell = new PdfPCell(paragraph1112);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].addressline3 != null) && (section7[i].addressline3.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph1113 = new Paragraph();
                                    paragraph1113.Add(ch1);
                                    paragraph1113.Add(chspace);
                                    paragraph1113.Add(ch2);
                                    cell = new PdfPCell(paragraph1113);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1114 = new Paragraph();
                                    //paragraph1114.Add();
                                    //cell = new PdfPCell(paragraph1114);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].postalcode != null) && (section7[i].postalcode.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph1115 = new Paragraph();
                                    paragraph1115.Add(ch1);
                                    paragraph1115.Add(chspace);
                                    paragraph1115.Add(ch2);
                                    cell = new PdfPCell(paragraph1115);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1116 = new Paragraph();
                                    //paragraph1116.Add();
                                    //cell = new PdfPCell(paragraph1116);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].country != null) && (section7[i].country.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].country, FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph1117 = new Paragraph();
                                    paragraph1117.Add(ch1);
                                    paragraph1117.Add(chspace);
                                    paragraph1117.Add(ch2);
                                    cell = new PdfPCell(paragraph1117);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1118 = new Paragraph();
                                    //paragraph1118.Add();
                                    //cell = new PdfPCell(paragraph1118);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section7[i].numberofshare != null) && (section7[i].numberofshare.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Number of Shares:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section7[i].numberofshare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph1121 = new Paragraph();
                                    paragraph1121.Add(ch1);
                                    paragraph1121.Add(chspace);
                                    paragraph1121.Add(ch2);
                                    cell = new PdfPCell(paragraph1121);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph1122 = new Paragraph();
                                    //paragraph1122.Add();
                                    //cell = new PdfPCell(paragraph1122);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }
                                #endregion CorporateSubscriber 1

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                            }



                            #endregion Corporate Subscriber
                        }
                    }

                    #endregion section7

                    #region section8
                    var section8 = db.cls_beneficialowner_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section8 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        if (section8.Count > 0)
                        {
                            #region Beneficial Owner Details

                            Paragraph paragraph2101 = new Paragraph();
                            paragraph2101.Add(new Chunk("Beneficial Owner Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                            cell = new PdfPCell(paragraph2101);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            //#region blank
                            ////Paragraph paragraph1blank = new Paragraph();
                            //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            //cell = new PdfPCell(paragraph1blank);
                            //cell.Colspan = 2;
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //#endregion blank
                            Chunk chspace = new Chunk("     ");
                            for (int i = 0; i < section8.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                                if (i == 3)
                                {
                                    Paragraph paragraph2101additionbal = new Paragraph();
                                    paragraph2101additionbal.Add(new Chunk("Additional Beneficial Owner Details (Individual)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph2101additionbal);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                #region  Beneficial Owner 1
                                //Beneficial Owner 1 
                                int n = 1 + i;

                                Paragraph paragraph2102 = new Paragraph();
                                paragraph2102.Add(new Chunk("Beneficial Owner " + n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph2102);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                if ((section8[i].name != null) && (section8[i].name.ToString() != ""))
                                {
                                    string[] names = section8[i].name.Split(' ');

                                    Chunk ch1 = new Chunk("First Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph2103 = new Paragraph();
                                    paragraph2103.Add(ch1);
                                    paragraph2103.Add(chspace);
                                    paragraph2103.Add(ch2);
                                    cell = new PdfPCell(paragraph2103);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2104 = new Paragraph();
                                    //paragraph2104.Add();
                                    //cell = new PdfPCell(paragraph2104);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);

                                    Chunk ch3 = new Chunk("Last Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch4 = new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2103lastname = new Paragraph();
                                    paragraph2103lastname.Add(ch3);
                                    paragraph2103lastname.Add(chspace);
                                    paragraph2103lastname.Add(ch4);
                                    cell = new PdfPCell(paragraph2103lastname);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2104lastname = new Paragraph();
                                    //paragraph2104lastname.Add();
                                    //cell = new PdfPCell(paragraph2104lastname);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].addressline1 != null) && (section8[i].addressline1.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2105 = new Paragraph();
                                    paragraph2105.Add(ch1);
                                    paragraph2105.Add(chspace);
                                    paragraph2105.Add(ch2);
                                    cell = new PdfPCell(paragraph2105);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2106 = new Paragraph();
                                    //paragraph2106.Add();
                                    //cell = new PdfPCell(paragraph2106);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].addressline2 != null) && (section8[i].addressline2.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph2107 = new Paragraph();
                                    paragraph2107.Add(ch1);
                                    paragraph2107.Add(chspace);
                                    paragraph2107.Add(ch2);
                                    cell = new PdfPCell(paragraph2107);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2108 = new Paragraph();
                                    //paragraph2108.Add();
                                    //cell = new PdfPCell(paragraph2108);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].addressline3 != null) && (section8[i].addressline3.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2109 = new Paragraph();
                                    paragraph2109.Add(ch1);
                                    paragraph2109.Add(chspace);
                                    paragraph2109.Add(ch2);
                                    cell = new PdfPCell(paragraph2109);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2110 = new Paragraph();
                                    //paragraph2110.Add();
                                    //cell = new PdfPCell(paragraph2110);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].postalcode != null) && (section8[i].postalcode.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2111 = new Paragraph();
                                    paragraph2111.Add(ch1);
                                    paragraph2111.Add(chspace);
                                    paragraph2111.Add(ch2);
                                    cell = new PdfPCell(paragraph2111);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2112 = new Paragraph();
                                    //paragraph2112.Add();
                                    //cell = new PdfPCell(paragraph2112);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].country != null) && (section8[i].country.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2113 = new Paragraph();
                                    paragraph2113.Add(ch1);
                                    paragraph2113.Add(chspace);
                                    paragraph2113.Add(ch2);
                                    cell = new PdfPCell(paragraph2113);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2114 = new Paragraph();
                                    //paragraph2114.Add();
                                    //cell = new PdfPCell(paragraph2114);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].nationality != null) && (section8[i].nationality.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Nationality:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].nationality.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                    Paragraph paragraph2115 = new Paragraph();
                                    paragraph2115.Add(ch1);
                                    paragraph2115.Add(chspace);
                                    paragraph2115.Add(ch2);
                                    cell = new PdfPCell(paragraph2115);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2116 = new Paragraph();
                                    //paragraph2116.Add(new );
                                    //cell = new PdfPCell(paragraph2116);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].occupation != null) && (section8[i].occupation.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Occupation:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].occupation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2117 = new Paragraph();
                                    paragraph2117.Add(ch1);
                                    paragraph2117.Add(chspace);
                                    paragraph2117.Add(ch2);
                                    cell = new PdfPCell(paragraph2117);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2118 = new Paragraph();
                                    //paragraph2118.Add();
                                    //cell = new PdfPCell(paragraph2118);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }

                                if ((section8[i].natureofownership != null) && (section8[i].natureofownership.ToString() != ""))
                                {
                                    Chunk ch1 = new Chunk("Nature of Beneficial Ownership:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                    Chunk ch2 = new Chunk(section8[i].natureofownership.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                    Paragraph paragraph2119 = new Paragraph();
                                    paragraph2119.Add(ch1);
                                    paragraph2119.Add(chspace);
                                    paragraph2119.Add(ch2);
                                    cell = new PdfPCell(paragraph2119);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    //Paragraph paragraph2120 = new Paragraph();
                                    //paragraph2120.Add();
                                    //cell = new PdfPCell(paragraph2120);
                                    //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    //cell.Padding = 0;
                                    //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //table1.AddCell(cell);
                                }
                                #endregion Beneficial Owner 1

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                            }



                            #endregion  Beneficial Owner Details
                        }
                    }
                    #endregion section8

                    #region section9
                    var section9 = db.cls_addressdetails_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section9 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        #region Address Details

                        Paragraph paragraph3101 = new Paragraph();
                        paragraph3101.Add(new Chunk("Address Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph3101);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph3102 = new Paragraph();
                        paragraph3102.Add(new Chunk("Registered Office Address", FontFactory.GetFont("Calibri", 12, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph3102);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        Chunk chspace = new Chunk("     ");
                        if ((section9.roaddressline1 != null) && (section9.roaddressline1.ToString() != ""))
                        {
                            Chunk ch5 = new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch6 = new Chunk(section9.roaddressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph3103 = new Paragraph();
                            paragraph3103.Add(ch5);
                            paragraph3103.Add(chspace);
                            paragraph3103.Add(ch6);
                            cell = new PdfPCell(paragraph3103);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph3104 = new Paragraph();
                            //paragraph3104.Add();
                            //cell = new PdfPCell(paragraph3104);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section9.roaddressline2 != null) && (section9.roaddressline2.ToString() != ""))
                        {
                            Chunk ch7 = new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch8 = new Chunk(section9.roaddressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph3105 = new Paragraph();
                            paragraph3105.Add(ch7);
                            paragraph3105.Add(chspace);
                            paragraph3105.Add(ch8);
                            cell = new PdfPCell(paragraph3105);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph3106 = new Paragraph();
                            //paragraph3106.Add();
                            //cell = new PdfPCell(paragraph3106);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section9.oraddressline3 != null) && (section9.oraddressline3.ToString() != ""))
                        {
                            Chunk ch9 = new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch10 = new Chunk(section9.oraddressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            Paragraph paragraph3107 = new Paragraph();
                            paragraph3107.Add(ch9);
                            paragraph3107.Add(chspace);
                            paragraph3107.Add(ch10);
                            cell = new PdfPCell(paragraph3107);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph3108 = new Paragraph();
                            //paragraph3108.Add();
                            //cell = new PdfPCell(paragraph3108);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section9.ropostalcode != null) && (section9.ropostalcode.ToString() != ""))
                        {
                            Chunk ch11 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch12 = new Chunk(section9.ropostalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                            Paragraph paragraph3109 = new Paragraph();
                            paragraph3109.Add(ch11);
                            paragraph3109.Add(chspace);
                            paragraph3109.Add(ch12);
                            cell = new PdfPCell(paragraph3109);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph3110 = new Paragraph();
                            //paragraph3110.Add();
                            //cell = new PdfPCell(paragraph3110);
                            //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        Chunk ch1 = new Chunk("The registered office address is also the business / central administration address: ", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                        Chunk ch2;
                        if (section9.roisalsothebusinessorcaaddress.ToString() != "")
                        {
                            ch2 = new Chunk(section9.roisalsothebusinessorcaaddress.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                        }
                        else
                        {
                            ch2 = new Chunk("No", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                        }
                        Paragraph paragraph3120 = new Paragraph();
                        paragraph3120.Add(ch1);
                        paragraph3120.Add(chspace);
                        paragraph3120.Add(ch2);
                        cell = new PdfPCell(paragraph3120);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        //Paragraph paragraph3121 = new Paragraph();
                        //paragraph3121.Add();
                        //cell = new PdfPCell(paragraph3121);
                        //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                        //cell.Padding = 0;
                        //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        //table1.AddCell(cell);

                        if (section9.roisalsothebusinessorcaaddress.ToString() != "Yes")
                        {
                            if ((section9.caaddressline1 != null) && (section9.caaddressline1.ToString() != ""))
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                                Paragraph paragraph3111 = new Paragraph();
                                paragraph3111.Add(new Chunk("Business / Central Administration Address (If Different from Registered Address)", FontFactory.GetFont("Calibri", 12, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3111);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                            }
                            if ((section9.caaddressline1 != null) && (section9.caaddressline1.ToString() != ""))
                            {
                                Chunk ch3 = new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                Chunk ch4 = new Chunk(section9.caaddressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                                Paragraph paragraph3112 = new Paragraph();
                                paragraph3112.Add(ch3);
                                paragraph3112.Add(chspace);
                                paragraph3112.Add(ch4);

                                cell = new PdfPCell(paragraph3112);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //Paragraph paragraph3113 = new Paragraph();
                                //paragraph3113.Add();
                                //cell = new PdfPCell(paragraph3113);
                                //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                            }

                            if ((section9.caaddressline2 != null) && (section9.caaddressline2.ToString() != ""))
                            {
                                Chunk ch3 = new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                Chunk ch4 = new Chunk(section9.caaddressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                Paragraph paragraph3114 = new Paragraph();
                                paragraph3114.Add(ch3);
                                paragraph3114.Add(chspace);
                                paragraph3114.Add(ch4);
                                cell = new PdfPCell(paragraph3114);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //Paragraph paragraph3115 = new Paragraph();
                                //paragraph3115.Add();
                                //cell = new PdfPCell(paragraph3115);
                                //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                            }

                            if ((section9.caaddressline3 != null) && (section9.caaddressline3.ToString() != ""))
                            {

                                Chunk ch3 = new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                Chunk ch4 = new Chunk(section9.caaddressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                Paragraph paragraph3116 = new Paragraph();
                                paragraph3116.Add(ch3);
                                paragraph3116.Add(chspace);
                                paragraph3116.Add(ch4);
                                cell = new PdfPCell(paragraph3116);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //Paragraph paragraph3117 = new Paragraph();
                                //paragraph3117.Add();
                                //cell = new PdfPCell(paragraph3117);
                                //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                            }

                            if ((section9.capostalcode != null) && (section9.capostalcode.ToString() != ""))
                            {
                                Chunk ch3 = new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                                Chunk ch4 = new Chunk(section9.capostalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));
                                Paragraph paragraph3118 = new Paragraph();
                                paragraph3118.Add(ch3);
                                paragraph3118.Add(chspace);
                                paragraph3118.Add(ch4);
                                cell = new PdfPCell(paragraph3118);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //Paragraph paragraph3119 = new Paragraph();
                                //paragraph3119.Add();
                                //cell = new PdfPCell(paragraph3119);
                                //cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                            }


                        }
                            
                        
                        #endregion Address Details
                    }
                    #endregion section9

                    #region section10
                    var section10 = db.cls_additionalinfo_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section10 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        #region additionalinfo
                        Paragraph paragraph4101 = new Paragraph();
                        paragraph4101.Add(new Chunk("Additional Information", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph4101);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                     
                        Paragraph paragraph4102 = new Paragraph();
                        paragraph4102.Add(new Chunk(section10.additionalinformation != null ? section10.additionalinformation.ToString():"" , FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE)));
                        cell = new PdfPCell(paragraph4102);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
      
                       


                        #endregion additionalinfo
                    }
                    #endregion section10

                    //table added to document
                    document.Add(table1);
                    //code to save pdf file in folder 
                    document.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();
                    var filename = Guid.NewGuid();
                    pdfFileName = filename.ToString() + ".pdf";
                    var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/OneDrive - CLS Chartered Secretaries/clscharteredsecretaries/" + companyFolderName + "/Submit"));
                    if (!exists)
                    {
                        Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/OneDrive - CLS Chartered Secretaries/clscharteredsecretaries/" + companyFolderName + "/Submit"));
                    }

                    filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/OneDrive - CLS Chartered Secretaries/clscharteredsecretaries/" + companyFolderName + "/Submit/" + filename + ".pdf");
                    System.IO.File.WriteAllBytes(filePath, bytes);
                }
                
            }
            return filePath;

        }

        private bool emailsend(string companyname,string pdfpath,decimal cfid)
        {
            WriteToFile("--------------------");
            WriteToFile("Mail send code begin");
            //string fromaddr = WebConfigurationManager.AppSettings["username"].ToString();
            //string toaddr = tomailid;//TO ADDRESS HERE
            //string password = WebConfigurationManager.AppSettings["pwd"].ToString();

            //MailMessage msg = new MailMessage();
            //msg.Subject = "Ticket";
            //msg.From = new MailAddress(fromaddr);
            //msg.Body = "Ticket Number : " + ticketno.ToString();
            //msg.To.Add(new MailAddress(toaddr));
            //SmtpClient smtp = new SmtpClient();
            //smtp.Host = "smtp.gmail.com";
            //smtp.Port = 587;
            //smtp.UseDefaultCredentials = false;
            //smtp.EnableSsl = true;
            //NetworkCredential nc = new NetworkCredential(fromaddr, password);
            //smtp.Credentials = nc;
            //smtp.Send(msg);
            try
            {
                string body = string.Empty;
                body = "Hello," + "<br><br>";

                body += "Contact Details for Incorporation Purposes" + "<br><br>";
                using (var db = new CompanyFormationdbEntities())
                {
                    var comincdetails = db.cls_agree_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (comincdetails != null)
                    {
                        string fullName = comincdetails.name != null ? comincdetails.name : "";
                        string companyName = comincdetails.companyname != null ? comincdetails.companyname : "";
                        string companyAddress = comincdetails.addressline1 != null ? comincdetails.addressline1 : "" + ", " + comincdetails.addressline2 != null ? comincdetails.addressline2 : "" + ", " + comincdetails.addressline3 != null ? comincdetails.addressline3 : "";
                        string postCode = comincdetails.postcode != null ? comincdetails.postcode : "";
                        string phoneNumber = comincdetails.phonenumber != null ? comincdetails.phonenumber : "";
                        string emailAddress = comincdetails.email != null ? comincdetails.email : "";
                        body += "FullName : " + fullName + "<br><br>";
                        body += "Company Name : " + companyName + "<br><br>";
                        body += "Address : " + companyAddress + "<br><br>";
                        body += "Postcode : " + postCode + "<br><br>";
                        body += "Phone No : " + phoneNumber + "<br><br>";
                        body += "Email : " + emailAddress + "<br><br>";
                        body += "Thank You";
                    }
                }

                MailMessage mail = new MailMessage();

                //mail.From.DisplayName = "CLS Chartered Secretaries (“CLS”)";
                mail.From = new MailAddress("formations@clscs.ie", "CLS Chartered Secretaries (“CLS”)");
                mail.To.Add(new MailAddress("ajay.zala@archesoftronix.com"));
                mail.Subject = "New Company Formation";
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.Attachments.Add(new Attachment(pdfpath));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.office365.com";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = new System.Net.NetworkCredential("formations@clscs.ie", "Sav42425", "");
                smtp.Send(mail);





                WriteToFile("Mail send");
                WriteToFile("--------------------");
                return true;
            }
            catch (Exception ex)
            {
                WriteToFile("Mail Exception");
                WriteToFile(ex.Message.ToString());
                WriteToFile(ex.InnerException.Message.ToString());
                WriteToFile("--------------------");
                return true;
            }
        }

        

        #region log

        private void WriteToFile(string text)
        {
            string logFileName = System.DateTime.Now.ToString("dd/MM/yyyy").Replace('/', '_').ToString() + ".log";
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

        /*
        #region oldcode
        private string CreatePDFBody(decimal cfid)
        {

            string filePath = string.Empty;

            using (var db = new CompanyFormationdbEntities())
            {
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    Paragraph paragraph1blank = new Paragraph();
                    iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 10, 10, 10, 10);

                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    PdfPTable table;
                    PdfPCell cell;
                    iTextSharp.text.Paragraph paragraph;

                    PdfPTable table1 = new PdfPTable(2);
                    table1.SetWidthPercentage(new float[2] { 460f, 140f }, PageSize.LETTER);

                    #region section1
                    var section1 = db.cls_agree_tbl.Where(x => x.cfid == cfid).FirstOrDefault();

                    if (section1 != null)
                    {
                        //First section
                        #region agree
                        companyFolderName = section1.companyname != null ? section1.companyname.ToString() : "Temp";
                        var FontColour = new BaseColor(77, 24, 111);
                        var TIMES_ROMAN20 = FontFactory.GetFont("Calibri", 30, Font.BOLD, FontColour);

                        Paragraph paragraph1 = new Paragraph();
                        paragraph1.Add(new Chunk("CLS Online Company Order Form", TIMES_ROMAN20));
                        cell = new PdfPCell(paragraph1);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if (section1.agree >= 0)
                        {
                            //Paragraph paragraph2 = new Paragraph();
                            //paragraph2.Add(new Chunk("Agree to the CLS General Terms of Business have reviewed the CLS Handy Guide to Completing the Company Order Form:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk("Agree to the CLS General Terms of Business have reviewed the CLS Handy Guide to Completing the Company Order Form:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section1.agree == 0 ? "Agree" : "DisAgree", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLUE));

                            //cell = new PdfPCell(paragraph2);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.PaddingRight = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            Paragraph paragraph3 = new Paragraph();
                            paragraph3.Add(ch1);
                            paragraph3.Add(ch2);
                            cell = new PdfPCell(paragraph3);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                            table1.AddCell(cell);

                        }

                        if ((section1.nonthirdparties != null) && (section1.nonthirdparties.ToString() != ""))
                        {
                            //Paragraph paragraph4 = new Paragraph();
                            //paragraph4.Add(new Chunk("Anti Money Laundering Customer Due Diligence: ", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk c1 = new Chunk("Anti Money Laundering Customer Due Diligence: ", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk c2 = new Chunk(section1.nonthirdparties.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK));
                            Paragraph paragraph5 = new Paragraph();
                            paragraph5.Add(c1 + " " + c2);
                            cell = new PdfPCell(new Phrase(c1 + " " + c2));
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph5 = new Paragraph();
                            //paragraph5.Add(new Chunk(section1.nonthirdparties.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph5);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section1.incorporationtype != null) && (section1.incorporationtype.ToString() != ""))
                        {
                            Paragraph paragraph6 = new Paragraph();
                            paragraph6.Add(new Chunk("Incorporation Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk("Incorporation Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));
                            Chunk ch2 = new Chunk(section1.incorporationtype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK));
                            Paragraph paragraph7 = new Paragraph();
                            paragraph7.Add(ch1 + " " + ch2);

                            cell = new PdfPCell(paragraph7);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph7 = new Paragraph();
                            //paragraph7.Add(new Chunk(section1.incorporationtype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph7);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section1.companypacktype != null) && (section1.companypacktype.ToString() != ""))
                        {

                            Paragraph paragraph9 = new Paragraph();
                            paragraph9.Add(new Chunk(section1.companypacktype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk(section1.companypacktype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK));
                            Chunk ch2 = new Chunk("Company Pack Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));

                            Paragraph paragraph8 = new Paragraph();
                            paragraph8.Add(ch2 + " " + ch1);
                            cell = new PdfPCell(paragraph8);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph9 = new Paragraph();
                            //paragraph9.Add(new Chunk(section1.companypacktype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph9);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }

                        if ((section1.paymenttype != null) && (section1.paymenttype.ToString() != ""))
                        {
                            Paragraph paragraph11 = new Paragraph();
                            paragraph11.Add(new Chunk(section1.paymenttype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            Chunk ch1 = new Chunk(section1.paymenttype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK));
                            Chunk ch2 = new Chunk("Payment Type (Select):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK));

                            Paragraph paragraph10 = new Paragraph();
                            paragraph10.Add(ch2 + " " + ch1);
                            cell = new PdfPCell(paragraph10);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12;
                            cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //Paragraph paragraph11 = new Paragraph();
                            //paragraph11.Add(new Chunk(section1.paymenttype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph11);
                            //cell.BorderWidth = 0;
                            //cell.BorderWidthBottom = 0.5f;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                        }
                        #endregion agree

                        #region Contact Details For Incorporation Purposes

                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph12 = new Paragraph();
                        paragraph12.Add(new Chunk("Contact Details For Incorporation Purposes", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph12);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if ((section1.name != null) && (section1.name.ToString() != ""))
                        {
                            //Paragraph paragraph13 = new Paragraph();
                            //paragraph13.Add(new Chunk("Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph13);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);

                            //Paragraph paragraph14 = new Paragraph();
                            //paragraph14.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            //cell = new PdfPCell(paragraph14);
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //code to split name
                            string[] names = section1.name.Split(' ');



                            Paragraph paragraph13firstname = new Paragraph();
                            paragraph13firstname.Add(new Chunk("Firstname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph13firstname);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph14firstnamevalue = new Paragraph();
                            paragraph14firstnamevalue.Add(new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph14firstnamevalue);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph13lastname = new Paragraph();
                            paragraph13lastname.Add(new Chunk("Lastname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph13lastname);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph14lastnamevalue = new Paragraph();
                            paragraph14lastnamevalue.Add(new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph14lastnamevalue);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);


                        }

                        if ((section1.companyname != null) && (section1.companyname.ToString() != ""))
                        {
                            Paragraph paragraph15 = new Paragraph();
                            paragraph15.Add(new Chunk("Practice/Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph15);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph16 = new Paragraph();
                            paragraph16.Add(new Chunk(section1.companyname.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph16);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                        }

                        if ((section1.addressline1 != null) && (section1.addressline1.ToString() != ""))
                        {
                            Paragraph paragraph17 = new Paragraph();
                            paragraph17.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph17);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph18 = new Paragraph();
                            paragraph18.Add(new Chunk(section1.addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph18);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.addressline2 != null) && (section1.addressline2.ToString() != ""))
                        {
                            Paragraph paragraph19 = new Paragraph();
                            paragraph19.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph19);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph20 = new Paragraph();
                            paragraph20.Add(new Chunk(section1.addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph20);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.addressline3 != null) && (section1.addressline3.ToString() != ""))
                        {
                            Paragraph paragraph21 = new Paragraph();
                            paragraph21.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph21);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph22 = new Paragraph();
                            paragraph22.Add(new Chunk(section1.addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph22);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.postcode != null) && (section1.postcode.ToString() != ""))
                        {
                            Paragraph paragraph23 = new Paragraph();
                            paragraph23.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph23);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph24 = new Paragraph();
                            paragraph24.Add(new Chunk(section1.postcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph24);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.phonenumber != null) && (section1.phonenumber.ToString() != ""))
                        {
                            Paragraph paragraph25 = new Paragraph();
                            paragraph25.Add(new Chunk("Phone Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph25);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph26 = new Paragraph();
                            paragraph26.Add(new Chunk(section1.phonenumber.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph26);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section1.email != null) && (section1.email.ToString() != ""))
                        {
                            Paragraph paragraph27 = new Paragraph();
                            paragraph27.Add(new Chunk("E-Mail Address:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph27);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph28 = new Paragraph();
                            paragraph28.Add(new Chunk(section1.email.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph28);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        #endregion Contact Details For Incorporation Purposes

                    }
                    #endregion section1

                    #region section2
                    var section2 = db.cls_companyincorporation_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section2 != null)
                    {
                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        #region Company Incorporation Required Details
                        Paragraph paragraph29 = new Paragraph();
                        paragraph29.Add(new Chunk("Company Incorporation Required Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph29);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        // Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph30 = new Paragraph();
                        paragraph30.Add(new Chunk("Proposed Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph30);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        if ((section2.firstchoice != null) && (section2.firstchoice.ToString() != ""))
                        {
                            Paragraph paragraph31 = new Paragraph();
                            paragraph31.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph31);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph32 = new Paragraph();
                            paragraph32.Add(new Chunk("First Choice", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph32);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph33 = new Paragraph();
                            paragraph33.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph33);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph34 = new Paragraph();
                            paragraph34.Add(new Chunk(section2.firstchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph34);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        if ((section2.secondchoice != null) && (section2.secondchoice.ToString() != ""))
                        {
                            Paragraph paragraph35 = new Paragraph();
                            paragraph35.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph35);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph36 = new Paragraph();
                            paragraph36.Add(new Chunk("Second Choice", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph36);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph37 = new Paragraph();
                            paragraph37.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph37);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph38 = new Paragraph();
                            paragraph38.Add(new Chunk(section2.secondchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph38);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        if ((section2.thirdchoice != null) && (section2.thirdchoice.ToString() != ""))
                        {
                            Paragraph paragraph39 = new Paragraph();
                            paragraph39.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph39);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph40 = new Paragraph();
                            paragraph40.Add(new Chunk("Third Choice", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph40);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph41 = new Paragraph();
                            paragraph41.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph41);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph42 = new Paragraph();
                            paragraph42.Add(new Chunk(section2.thirdchoice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph42);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        if ((section2.principalactivity != null) && (section2.principalactivity.ToString() != ""))
                        {

                            Paragraph paragraph44 = new Paragraph();
                            paragraph44.Add(new Chunk("Principal Activity of the Company:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph44);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph45 = new Paragraph();
                            paragraph45.Add(new Chunk("Principal Activity", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph45);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph46 = new Paragraph();
                            paragraph46.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph46);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph47 = new Paragraph();
                            paragraph47.Add(new Chunk(section2.principalactivity.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph47);
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            if ((section2.additionalwording != null) && (section2.additionalwording.ToString() != ""))
                            {
                                Paragraph paragraph44additionalwording = new Paragraph();
                                paragraph44additionalwording.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph44additionalwording);
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph45additionalwording = new Paragraph();
                                paragraph45additionalwording.Add(new Chunk("Additional Wording", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph45additionalwording);
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph46additionalwording = new Paragraph();
                                paragraph46additionalwording.Add(new Chunk("", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph46additionalwording);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph47additionalwording = new Paragraph();
                                paragraph47additionalwording.Add(new Chunk(section2.additionalwording.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph47additionalwording);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                            }


                        }
                        if ((section2.companytype != null) && (section2.companytype.ToString() != ""))
                        {
                            Paragraph paragraph48 = new Paragraph();
                            paragraph48.Add(new Chunk("Company Type:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph48);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph49 = new Paragraph();
                            paragraph49.Add(new Chunk(section2.companytype.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph49);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        #endregion Company Incorporation Required Details
                    }
                    #endregion section2

                    #region section3
                    #region Share Capital
                    var section3 = db.cls_sharecapital_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section3 != null)
                    {

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph50 = new Paragraph();
                        paragraph50.Add(new Chunk("Share Capital", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph50);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if ((section3.issuedsharecapital != null) && (section3.issuedsharecapital.ToString() != ""))
                        {
                            Paragraph paragraph51 = new Paragraph();
                            paragraph51.Add(new Chunk("Issued Share Capital:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph51);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph52 = new Paragraph();
                            paragraph52.Add(new Chunk(section3.issuedsharecapital.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph52);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section3.nominalamoutpershare != null) && (section3.nominalamoutpershare.ToString() != ""))
                        {

                            Paragraph paragraph53 = new Paragraph();
                            paragraph53.Add(new Chunk("Nominal Amount Per Share::", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph53);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph54 = new Paragraph();
                            paragraph54.Add(new Chunk(section3.nominalamoutpershare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph54);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section3.shareclass != null) && (section3.shareclass.ToString() != ""))
                        {
                            Paragraph paragraph55 = new Paragraph();
                            paragraph55.Add(new Chunk("Share Class:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph55);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph56 = new Paragraph();
                            paragraph56.Add(new Chunk(section3.shareclass.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph56);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section3.authorisedsharecapital != null) && (section3.authorisedsharecapital.ToString() != ""))
                        {
                            Paragraph paragraph57 = new Paragraph();
                            paragraph57.Add(new Chunk("Authorised Share Capital (Optional for LTD):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph57);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph58 = new Paragraph();
                            paragraph58.Add(new Chunk(section3.authorisedsharecapital.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph58);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                    }
                    #endregion Share Capital
                    #endregion section3

                    #region section4

                    var section4 = db.cls_secretary_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section4 != null)
                    {
                        #region Company Secretary Details

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph59 = new Paragraph();
                        paragraph59.Add(new Chunk("Company Secretary Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph59);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph60 = new Paragraph();
                        paragraph60.Add(new Chunk("Individual Secretary Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph60);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if ((section4.name != null) && (section4.name.ToString() != ""))
                        {
                            string[] names = section4.name.Split(' ');

                            Paragraph paragraph61 = new Paragraph();
                            paragraph61.Add(new Chunk("Firstname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph61);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph62 = new Paragraph();
                            paragraph62.Add(new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph62);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph61lastname = new Paragraph();
                            paragraph61lastname.Add(new Chunk("Lastname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph61lastname);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph62lastname = new Paragraph();
                            paragraph62lastname.Add(new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph62lastname);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                        }

                        if ((section4.dob != null) && (section4.dob.ToString() != ""))
                        {
                            Paragraph paragraph63 = new Paragraph();
                            paragraph63.Add(new Chunk("Date of Birth:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph63);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph64 = new Paragraph();
                            paragraph64.Add(new Chunk(section4.dob != null ? Convert.ToDateTime(section4.dob).ToString("dd-MM-yyyy").Replace('-', '/') : "", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph64);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.addressline1 != null) && (section4.addressline1.ToString() != ""))
                        {
                            Paragraph paragraph65 = new Paragraph();
                            paragraph65.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph65);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph66 = new Paragraph();
                            paragraph66.Add(new Chunk(section4.addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph66);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.addressline2 != null) && (section4.addressline2.ToString() != ""))
                        {
                            Paragraph paragraph67 = new Paragraph();
                            paragraph67.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph67);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph68 = new Paragraph();
                            paragraph68.Add(new Chunk(section4.addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph68);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.addressline3 != null) && (section4.addressline3.ToString() != ""))
                        {
                            Paragraph paragraph69 = new Paragraph();
                            paragraph69.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph69);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph70 = new Paragraph();
                            paragraph70.Add(new Chunk(section4.addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph70);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.postalcode != null) && (section4.postalcode.ToString() != ""))
                        {
                            Paragraph paragraph71 = new Paragraph();
                            paragraph71.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph71);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph72 = new Paragraph();
                            paragraph72.Add(new Chunk(section4.postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph72);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.country != null) && (section4.country.ToString() != ""))
                        {
                            Paragraph paragraph73 = new Paragraph();
                            paragraph73.Add(new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph73);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph74 = new Paragraph();
                            paragraph74.Add(new Chunk(section4.country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph74);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.companyname != null) && (section4.companyname.ToString() != ""))
                        {

                            #region blank
                            //Paragraph paragraph1blank = new Paragraph();
                            paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            cell = new PdfPCell(paragraph1blank);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            #endregion blank

                            Paragraph paragraph75 = new Paragraph();
                            paragraph75.Add(new Chunk("Corporate Secretary Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph75);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);


                            #region blank
                            //Paragraph paragraph1blank = new Paragraph();
                            paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            cell = new PdfPCell(paragraph1blank);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            #endregion blank

                            Paragraph paragraph76 = new Paragraph();
                            paragraph76.Add(new Chunk("Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph76);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph77 = new Paragraph();
                            paragraph77.Add(new Chunk(section4.companyname.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph77);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.companynumber != null) && (section4.companynumber.ToString() != ""))
                        {
                            Paragraph paragraph78 = new Paragraph();
                            paragraph78.Add(new Chunk("Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph78);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph79 = new Paragraph();
                            paragraph79.Add(new Chunk(section4.companynumber.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph79);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.companydirector != null) && (section4.companydirector.ToString() != ""))
                        {
                            Paragraph paragraph80 = new Paragraph();
                            paragraph80.Add(new Chunk("Company Director (signing on behalf of the Company):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph80);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph81 = new Paragraph();
                            paragraph81.Add(new Chunk(section4.companydirector.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph81);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.companyregionaloffice != null) && (section4.companyregionaloffice.ToString() != ""))
                        {
                            Paragraph paragraph82 = new Paragraph();
                            paragraph82.Add(new Chunk("Registered Office:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph82);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph83 = new Paragraph();
                            paragraph83.Add(new Chunk(section4.companyregionaloffice.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph83);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.companyaddressline1 != null) && (section4.companyaddressline1.ToString() != ""))
                        {
                            Paragraph paragraph84 = new Paragraph();
                            paragraph84.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph84);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph85 = new Paragraph();
                            paragraph85.Add(new Chunk(section4.companyaddressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph85);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.companyaddressline2 != null) && (section4.companyaddressline2.ToString() != ""))
                        {
                            Paragraph paragraph86 = new Paragraph();
                            paragraph86.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph86);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph87 = new Paragraph();
                            paragraph87.Add(new Chunk(section4.companyaddressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph87);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.compnaypostal != null) && (section4.compnaypostal.ToString() != ""))
                        {
                            Paragraph paragraph88 = new Paragraph();
                            paragraph88.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph88);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph89 = new Paragraph();
                            paragraph89.Add(new Chunk(section4.compnaypostal.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph89);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section4.compnaycountry != null) && (section4.compnaycountry.ToString() != ""))
                        {
                            Paragraph paragraph90 = new Paragraph();
                            paragraph90.Add(new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph90);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph91 = new Paragraph();
                            paragraph91.Add(new Chunk(section4.compnaycountry.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph91);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }
                        #endregion Company Secretary Details
                    }
                    #endregion section4

                    #region section5
                    var section5 = db.cls_director_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section5 != null)
                    {
                        #region director


                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph92 = new Paragraph();
                        paragraph92.Add(new Chunk("Company Director(s)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph92);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        if (section5.Count > 0)
                        {
                            for (int i = 0; i < section5.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                                if (i == 3)
                                {



                                    Paragraph paragraph92addtional = new Paragraph();
                                    paragraph92addtional.Add(new Chunk("Additional Director Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph92addtional);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);



                                }
                                #region director 1
                                //director 1
                                int n = 1 + i;

                                Paragraph paragraph93 = new Paragraph();
                                paragraph93.Add(new Chunk("Director " + n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph93);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                //#region blank
                                ////Paragraph paragraph1blank = new Paragraph();
                                //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                //cell = new PdfPCell(paragraph1blank);
                                //cell.Colspan = 2;
                                //cell.BorderWidth = 0;
                                //cell.Padding = 0;
                                //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                //table1.AddCell(cell);
                                //#endregion blank
                                if ((section5[i].name != null) && (section5[i].name.ToString() != ""))
                                {
                                    string[] names = section5[i].name.Split(' ');

                                    Paragraph paragraph94 = new Paragraph();
                                    paragraph94.Add(new Chunk("Firstname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph94);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph95 = new Paragraph();
                                    paragraph95.Add(new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph95);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph94lastname = new Paragraph();
                                    paragraph94lastname.Add(new Chunk("Lastname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph94lastname);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph95lastname = new Paragraph();
                                    paragraph95lastname.Add(new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph95lastname);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].dob != null) && (section5[i].dob.ToString() != ""))
                                {
                                    Paragraph paragraph96 = new Paragraph();
                                    paragraph96.Add(new Chunk("Date of Birth:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph96);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph97 = new Paragraph();
                                    paragraph97.Add(new Chunk(section5[i].dob != null ? Convert.ToDateTime(section5[i].dob.ToString()).ToString("dd-MM-yyyy").Replace('-', '/') : "", FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph97);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].occupation != null) && (section5[i].occupation.ToString() != ""))
                                {
                                    Paragraph paragraph98 = new Paragraph();
                                    paragraph98.Add(new Chunk("Occupation:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph98);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph99 = new Paragraph();
                                    paragraph99.Add(new Chunk(section5[i].occupation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph99);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].addressline1 != null) && (section5[i].addressline1.ToString() != ""))
                                {
                                    Paragraph paragraph100 = new Paragraph();
                                    paragraph100.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph100);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph101 = new Paragraph();
                                    paragraph101.Add(new Chunk(section5[i].addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph101);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].addressline2 != null) && (section5[i].addressline2.ToString() != ""))
                                {
                                    Paragraph paragraph102 = new Paragraph();
                                    paragraph102.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph102);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph103 = new Paragraph();
                                    paragraph103.Add(new Chunk(section5[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph103);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].addressline3 != null) && (section5[i].addressline3.ToString() != ""))
                                {
                                    Paragraph paragraph104 = new Paragraph();
                                    paragraph104.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph104);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph105 = new Paragraph();
                                    paragraph105.Add(new Chunk(section5[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph105);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].postalcode != null) && (section5[i].postalcode.ToString() != ""))
                                {
                                    Paragraph paragraph106 = new Paragraph();
                                    paragraph106.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph106);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph107 = new Paragraph();
                                    paragraph107.Add(new Chunk(section5[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph107);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].country != null) && (section5[i].country.ToString() != ""))
                                {
                                    Paragraph paragraph108 = new Paragraph();
                                    paragraph108.Add(new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph108);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph109 = new Paragraph();
                                    paragraph109.Add(new Chunk(section5[i].country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph109);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].nationality != null) && (section5[i].nationality.ToString() != ""))
                                {
                                    Paragraph paragraph110 = new Paragraph();
                                    paragraph110.Add(new Chunk("Nationality:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph110);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph111 = new Paragraph();
                                    paragraph111.Add(new Chunk(section5[i].nationality.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph111);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].otherdirectorship1 != null) && (section5[i].otherdirectorship1.ToString() != ""))
                                {
                                    Paragraph paragraph112 = new Paragraph();
                                    paragraph112.Add(new Chunk("Other Directorship 1 - Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph112);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph113 = new Paragraph();
                                    paragraph113.Add(new Chunk(section5[i].otherdirectorship1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph113);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].otherdirectorship2 != null) && (section5[i].otherdirectorship2.ToString() != ""))
                                {
                                    Paragraph paragraph114 = new Paragraph();
                                    paragraph114.Add(new Chunk("Other Directorship 2 - Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph114);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph115 = new Paragraph();
                                    paragraph115.Add(new Chunk(section5[i].otherdirectorship2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph115);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].otherdirectorship3 != null) && (section5[i].otherdirectorship3.ToString() != ""))
                                {
                                    Paragraph paragraph116 = new Paragraph();
                                    paragraph116.Add(new Chunk("Other Directorship 3 - Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph116);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph117 = new Paragraph();
                                    paragraph117.Add(new Chunk(section5[i].otherdirectorship3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph117);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].restricted != null) && (section5[i].restricted.ToString() != ""))
                                {
                                    Paragraph paragraph118 = new Paragraph();
                                    paragraph118.Add(new Chunk("Disqualified or Restricted:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph118);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph119 = new Paragraph();
                                    paragraph119.Add(new Chunk(section5[i].restricted.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph119);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].numberofshare != null) && (section5[i].numberofshare.ToString() != ""))
                                {
                                    Paragraph paragraph120 = new Paragraph();
                                    paragraph120.Add(new Chunk("If this director is also a subscriber, enter their number of shares:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph120);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph121 = new Paragraph();
                                    paragraph121.Add(new Chunk(section5[i].numberofshare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph121);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section5[i].beneficialowner != null) && (section5[i].beneficialowner.ToString() != ""))
                                {
                                    Paragraph paragraph122 = new Paragraph();
                                    paragraph122.Add(new Chunk("Is the director the beneficial owner of the above shares?", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph122);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph123 = new Paragraph();
                                    paragraph123.Add(new Chunk(section5[i].beneficialowner.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph123);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }
                                #endregion director 1
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                            }

                        }

                        #endregion director
                    }
                    #endregion section5

                    #region section6
                    var section6 = db.cls_subscriber_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section6 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        if (section6.Count > 0)
                        {
                            #region Subscriber Details (Individual)
                            Paragraph paragraph311 = new Paragraph();
                            paragraph311.Add(new Chunk("Subscriber Details (Individual)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                            cell = new PdfPCell(paragraph311);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //#region blank
                            //Paragraph paragraph1blank = new Paragraph();
                            //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            //cell = new PdfPCell(paragraph1blank);
                            //cell.Colspan = 2;
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //#endregion blank


                            for (int i = 0; i < section6.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                                if (i == 3)
                                {
                                    Paragraph paragraph311additional = new Paragraph();
                                    paragraph311additional.Add(new Chunk("Additional Subscriber Details (Individual)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph311additional);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }
                                #region  Subscriber 1
                                //director 7
                                int n = 1 + i;
                                Paragraph paragraph312 = new Paragraph();
                                paragraph312.Add(new Chunk("Subscriber " + n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph312);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                if ((section6[i].name != null) && (section6[i].name.ToString() != null))
                                {
                                    string[] names = section6[i].name.Split(' ');

                                    Paragraph paragraph313 = new Paragraph();
                                    paragraph313.Add(new Chunk("Firstame:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph313);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph314 = new Paragraph();
                                    paragraph314.Add(new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph314);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph313lastname = new Paragraph();
                                    paragraph313lastname.Add(new Chunk("Lastname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph313lastname);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph314lastname = new Paragraph();
                                    paragraph314lastname.Add(new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph314lastname);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                }

                                if ((section6[i].addressline1 != null) && (section6[i].addressline1.ToString() != null))
                                {
                                    Paragraph paragraph315 = new Paragraph();
                                    paragraph315.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph315);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph316 = new Paragraph();
                                    paragraph316.Add(new Chunk(section6[i].addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph316);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].addressline2 != null) && (section6[i].addressline2.ToString() != null))
                                {
                                    Paragraph paragraph317 = new Paragraph();
                                    paragraph317.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph317);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph318 = new Paragraph();
                                    paragraph318.Add(new Chunk(section6[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph318);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].addressline3 != null) && (section6[i].addressline3.ToString() != null))
                                {
                                    Paragraph paragraph319 = new Paragraph();
                                    paragraph319.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph319);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph320 = new Paragraph();
                                    paragraph320.Add(new Chunk(section6[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph320);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].postalcode != null) && (section6[i].postalcode.ToString() != null))
                                {
                                    Paragraph paragraph321 = new Paragraph();
                                    paragraph321.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph321);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph322 = new Paragraph();
                                    paragraph322.Add(new Chunk(section6[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph322);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].country != null) && (section6[i].country.ToString() != null))
                                {
                                    Paragraph paragraph323 = new Paragraph();
                                    paragraph323.Add(new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph323);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph324 = new Paragraph();
                                    paragraph324.Add(new Chunk(section6[i].country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph324);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].nationality != null) && (section6[i].nationality.ToString() != null))
                                {
                                    Paragraph paragraph325 = new Paragraph();
                                    paragraph325.Add(new Chunk("Nationality:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph325);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph326 = new Paragraph();
                                    paragraph326.Add(new Chunk(section6[i].nationality.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph326);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].occupation != null) && (section6[i].occupation.ToString() != null))
                                {
                                    Paragraph paragraph327 = new Paragraph();
                                    paragraph327.Add(new Chunk("Occupation:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph327);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph328 = new Paragraph();
                                    paragraph328.Add(new Chunk(section6[i].occupation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph328);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].numberofshare != null) && (section6[i].numberofshare.ToString() != null))
                                {
                                    Paragraph paragraph329 = new Paragraph();
                                    paragraph329.Add(new Chunk("Number of Shares:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph329);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph330 = new Paragraph();
                                    paragraph330.Add(new Chunk(section6[i].numberofshare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph330);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section6[i].beneficialowner != null) && (section6[i].beneficialowner.ToString() != null))
                                {
                                    Paragraph paragraph331 = new Paragraph();
                                    paragraph331.Add(new Chunk("Is the subscriber the beneficial owner of the shares?:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph331);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph332 = new Paragraph();
                                    paragraph332.Add(new Chunk(section6[i].beneficialowner.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph332);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }
                                #endregion Subscriber 1

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                            }
                            #endregion Subscriber Details (Individual)
                        }

                    }
                    #endregion section6

                    #region section7
                    var section7 = db.cls_corporatesubscriber_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section7 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        if (section7.Count > 0)
                        {
                            #region Corporate Subscriber
                            Paragraph paragraph1101 = new Paragraph();
                            paragraph1101.Add(new Chunk("Corporate Subscriber Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                            cell = new PdfPCell(paragraph1101);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            //#region blank
                            ////Paragraph paragraph1blank = new Paragraph();
                            //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            //cell = new PdfPCell(paragraph1blank);
                            //cell.Colspan = 2;
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //#endregion blank

                            for (int i = 0; i < section7.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                                if (i == 3)
                                {
                                    Paragraph paragraph1101addtionalcorporatesubscriber = new Paragraph();
                                    paragraph1101addtionalcorporatesubscriber.Add(new Chunk("Additional Corporate Subscriber Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph1101addtionalcorporatesubscriber);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                #region  CorporateSubscriber 1
                                int n = 1 + i;
                                Paragraph paragraph1102 = new Paragraph();
                                paragraph1102.Add(new Chunk("Corporate Subscriber " + n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph1102);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                if ((section7[i].companyname != null) && (section7[i].companyname.ToString() != ""))
                                {
                                    Paragraph paragraph1103 = new Paragraph();
                                    paragraph1103.Add(new Chunk("Company Name:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1103);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1104 = new Paragraph();
                                    paragraph1104.Add(new Chunk(section7[i].companyname.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1104);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                }

                                if ((section7[i].companyphonenumber != null) && (section7[i].companyphonenumber.ToString() != ""))
                                {
                                    Paragraph paragraph1105 = new Paragraph();
                                    paragraph1105.Add(new Chunk("Company Number:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1105);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1106 = new Paragraph();
                                    paragraph1106.Add(new Chunk(section7[i].companyphonenumber.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1106);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].companydirector != null) && (section7[i].companydirector.ToString() != ""))
                                {
                                    Paragraph paragraph1107 = new Paragraph();
                                    paragraph1107.Add(new Chunk("Company Director (Signing):", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1107);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1108 = new Paragraph();
                                    paragraph1108.Add(new Chunk(section7[i].companydirector.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1108);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].registerofficeaddress != null) && (section7[i].registerofficeaddress.ToString() != ""))
                                {
                                    Paragraph paragraph1109 = new Paragraph();
                                    paragraph1109.Add(new Chunk("Registered Office Address:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1109);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1110 = new Paragraph();
                                    paragraph1110.Add(new Chunk(section7[i].registerofficeaddress.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1110);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].addressline2 != null) && (section7[i].addressline2.ToString() != ""))
                                {
                                    Paragraph paragraph1111 = new Paragraph();
                                    paragraph1111.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1111);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1112 = new Paragraph();
                                    paragraph1112.Add(new Chunk(section7[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1112);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].addressline3 != null) && (section7[i].addressline3.ToString() != ""))
                                {
                                    Paragraph paragraph1113 = new Paragraph();
                                    paragraph1113.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1113);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1114 = new Paragraph();
                                    paragraph1114.Add(new Chunk(section7[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1114);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].postalcode != null) && (section7[i].postalcode.ToString() != ""))
                                {
                                    Paragraph paragraph1115 = new Paragraph();
                                    paragraph1115.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1115);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1116 = new Paragraph();
                                    paragraph1116.Add(new Chunk(section7[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1116);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].country != null) && (section7[i].country.ToString() != ""))
                                {
                                    Paragraph paragraph1117 = new Paragraph();
                                    paragraph1117.Add(new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1117);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1118 = new Paragraph();
                                    paragraph1118.Add(new Chunk(section7[i].country, FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1118);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section7[i].numberofshare != null) && (section7[i].numberofshare.ToString() != ""))
                                {
                                    Paragraph paragraph1121 = new Paragraph();
                                    paragraph1121.Add(new Chunk("Number of Shares:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1121);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph1122 = new Paragraph();
                                    paragraph1122.Add(new Chunk(section7[i].numberofshare.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph1122);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }
                                #endregion CorporateSubscriber 1

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                            }



                            #endregion Corporate Subscriber
                        }
                    }

                    #endregion section7

                    #region section8
                    var section8 = db.cls_beneficialowner_tbl.Where(x => x.cfid == cfid).ToList();
                    if (section8 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        if (section8.Count > 0)
                        {
                            #region Beneficial Owner Details

                            Paragraph paragraph2101 = new Paragraph();
                            paragraph2101.Add(new Chunk("Beneficial Owner Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                            cell = new PdfPCell(paragraph2101);
                            cell.Colspan = 2;
                            cell.BorderWidth = 0;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                            //#region blank
                            ////Paragraph paragraph1blank = new Paragraph();
                            //paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                            //cell = new PdfPCell(paragraph1blank);
                            //cell.Colspan = 2;
                            //cell.BorderWidth = 0;
                            //cell.Padding = 0;
                            //cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            //table1.AddCell(cell);
                            //#endregion blank
                            for (int i = 0; i < section8.Count; i++)
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                                if (i == 3)
                                {
                                    Paragraph paragraph2101additionbal = new Paragraph();
                                    paragraph2101additionbal.Add(new Chunk("Additional Beneficial Owner Details (Individual)", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                                    cell = new PdfPCell(paragraph2101additionbal);
                                    cell.Colspan = 2;
                                    cell.BorderWidth = 0;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                #region  Beneficial Owner 1
                                //Beneficial Owner 1 
                                int n = 1 + i;

                                Paragraph paragraph2102 = new Paragraph();
                                paragraph2102.Add(new Chunk("Beneficial Owner " + n.ToString(), FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph2102);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                if ((section8[i].name != null) && (section8[i].name.ToString() != ""))
                                {
                                    string[] names = section8[i].name.Split(' ');

                                    Paragraph paragraph2103 = new Paragraph();
                                    paragraph2103.Add(new Chunk("Firstname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2103);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2104 = new Paragraph();
                                    paragraph2104.Add(new Chunk(names[0].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2104);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2103lastname = new Paragraph();
                                    paragraph2103lastname.Add(new Chunk("Lastname:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2103lastname);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2104lastname = new Paragraph();
                                    paragraph2104lastname.Add(new Chunk(names[1].ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2104lastname);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].addressline1 != null) && (section8[i].addressline1.ToString() != ""))
                                {
                                    Paragraph paragraph2105 = new Paragraph();
                                    paragraph2105.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2105);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2106 = new Paragraph();
                                    paragraph2106.Add(new Chunk(section8[i].addressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2106);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].addressline2 != null) && (section8[i].addressline2.ToString() != ""))
                                {
                                    Paragraph paragraph2107 = new Paragraph();
                                    paragraph2107.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2107);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2108 = new Paragraph();
                                    paragraph2108.Add(new Chunk(section8[i].addressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2108);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].addressline3 != null) && (section8[i].addressline3.ToString() != ""))
                                {
                                    Paragraph paragraph2109 = new Paragraph();
                                    paragraph2109.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2109);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2110 = new Paragraph();
                                    paragraph2110.Add(new Chunk(section8[i].addressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2110);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].postalcode != null) && (section8[i].postalcode.ToString() != ""))
                                {
                                    Paragraph paragraph2111 = new Paragraph();
                                    paragraph2111.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2111);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2112 = new Paragraph();
                                    paragraph2112.Add(new Chunk(section8[i].postalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2112);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].country != null) && (section8[i].country.ToString() != ""))
                                {
                                    Paragraph paragraph2113 = new Paragraph();
                                    paragraph2113.Add(new Chunk("Country:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2113);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2114 = new Paragraph();
                                    paragraph2114.Add(new Chunk(section8[i].country.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2114);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].nationality != null) && (section8[i].nationality.ToString() != ""))
                                {
                                    Paragraph paragraph2115 = new Paragraph();
                                    paragraph2115.Add(new Chunk("Nationality:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2115);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2116 = new Paragraph();
                                    paragraph2116.Add(new Chunk(section8[i].nationality.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2116);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].occupation != null) && (section8[i].occupation.ToString() != ""))
                                {
                                    Paragraph paragraph2117 = new Paragraph();
                                    paragraph2117.Add(new Chunk("Occupation:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2117);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2118 = new Paragraph();
                                    paragraph2118.Add(new Chunk(section8[i].occupation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2118);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }

                                if ((section8[i].natureofownership != null) && (section8[i].natureofownership.ToString() != ""))
                                {
                                    Paragraph paragraph2119 = new Paragraph();
                                    paragraph2119.Add(new Chunk("Nature of Beneficial Ownership:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2119);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);

                                    Paragraph paragraph2120 = new Paragraph();
                                    paragraph2120.Add(new Chunk(section8[i].natureofownership.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                    cell = new PdfPCell(paragraph2120);
                                    cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                    cell.Padding = 0;
                                    cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    table1.AddCell(cell);
                                }
                                #endregion Beneficial Owner 1

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank
                            }



                            #endregion  Beneficial Owner Details
                        }
                    }
                    #endregion section8

                    #region section9
                    var section9 = db.cls_addressdetails_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section9 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank
                        #region Address Details

                        Paragraph paragraph3101 = new Paragraph();
                        paragraph3101.Add(new Chunk("Address Details", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph3101);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph3102 = new Paragraph();
                        paragraph3102.Add(new Chunk("Registered Office Address", FontFactory.GetFont("Calibri", 12, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph3102);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        if ((section9.roaddressline1 != null) && (section9.roaddressline1.ToString() != ""))
                        {
                            Paragraph paragraph3103 = new Paragraph();
                            paragraph3103.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3103);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph3104 = new Paragraph();
                            paragraph3104.Add(new Chunk(section9.roaddressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3104);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section9.roaddressline2 != null) && (section9.roaddressline2.ToString() != ""))
                        {
                            Paragraph paragraph3105 = new Paragraph();
                            paragraph3105.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3105);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph3106 = new Paragraph();
                            paragraph3106.Add(new Chunk(section9.roaddressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3106);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section9.oraddressline3 != null) && (section9.oraddressline3.ToString() != ""))
                        {
                            Paragraph paragraph3107 = new Paragraph();
                            paragraph3107.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3107);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph3108 = new Paragraph();
                            paragraph3108.Add(new Chunk(section9.oraddressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3108);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        if ((section9.ropostalcode != null) && (section9.ropostalcode.ToString() != ""))
                        {
                            Paragraph paragraph3109 = new Paragraph();
                            paragraph3109.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3109);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);

                            Paragraph paragraph3110 = new Paragraph();
                            paragraph3110.Add(new Chunk(section9.ropostalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                            cell = new PdfPCell(paragraph3110);
                            cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                            cell.Padding = 0;
                            cell.PaddingTop = 12; cell.PaddingBottom = 12;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.AddCell(cell);
                        }

                        Paragraph paragraph3120 = new Paragraph();
                        paragraph3120.Add(new Chunk("The registered office address is also the business / central administration address: ", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph3120);
                        cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        Paragraph paragraph3121 = new Paragraph();
                        paragraph3121.Add(new Chunk(section9.roisalsothebusinessorcaaddress.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph3121);
                        cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        if (section9.roisalsothebusinessorcaaddress.ToString() != "Yes")
                        {
                            if ((section9.caaddressline1 != null) && (section9.caaddressline1.ToString() != ""))
                            {
                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                                Paragraph paragraph3111 = new Paragraph();
                                paragraph3111.Add(new Chunk("Business / Central Administration Address (If Different from Registered Address)", FontFactory.GetFont("Calibri", 12, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3111);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                #region blank
                                //Paragraph paragraph1blank = new Paragraph();
                                paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                                cell = new PdfPCell(paragraph1blank);
                                cell.Colspan = 2;
                                cell.BorderWidth = 0;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                                #endregion blank

                            }
                            if ((section9.caaddressline1 != null) && (section9.caaddressline1.ToString() != ""))
                            {
                                Paragraph paragraph3112 = new Paragraph();
                                paragraph3112.Add(new Chunk("Address Line 1:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3112);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph3113 = new Paragraph();
                                paragraph3113.Add(new Chunk(section9.caaddressline1.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3113);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                            }

                            if ((section9.caaddressline2 != null) && (section9.caaddressline2.ToString() != ""))
                            {
                                Paragraph paragraph3114 = new Paragraph();
                                paragraph3114.Add(new Chunk("Address Line 2:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3114);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph3115 = new Paragraph();
                                paragraph3115.Add(new Chunk(section9.caaddressline2.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3115);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                            }

                            if ((section9.caaddressline3 != null) && (section9.caaddressline3.ToString() != ""))
                            {
                                Paragraph paragraph3116 = new Paragraph();
                                paragraph3116.Add(new Chunk("Address Line 3:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3116);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph3117 = new Paragraph();
                                paragraph3117.Add(new Chunk(section9.caaddressline3.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3117);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                            }

                            if ((section9.capostalcode != null) && (section9.capostalcode.ToString() != ""))
                            {
                                Paragraph paragraph3118 = new Paragraph();
                                paragraph3118.Add(new Chunk("Eircode/Postcode:", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3118);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);

                                Paragraph paragraph3119 = new Paragraph();
                                paragraph3119.Add(new Chunk(section9.capostalcode.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                                cell = new PdfPCell(paragraph3119);
                                cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                                cell.Padding = 0;
                                cell.PaddingTop = 12; cell.PaddingBottom = 12;
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                table1.AddCell(cell);
                            }


                        }


                        #endregion Address Details
                    }
                    #endregion section9

                    #region section10
                    var section10 = db.cls_additionalinfo_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (section10 != null)
                    {
                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        #region additionalinfo
                        Paragraph paragraph4101 = new Paragraph();
                        paragraph4101.Add(new Chunk("Additional Information", FontFactory.GetFont("Calibri", 20, Font.BOLD, BaseColor.RED)));
                        cell = new PdfPCell(paragraph4101);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);

                        #region blank
                        //Paragraph paragraph1blank = new Paragraph();
                        paragraph1blank.Add(new Chunk("*", FontFactory.GetFont("Calibri", 15, Font.BOLD, BaseColor.WHITE)));
                        cell = new PdfPCell(paragraph1blank);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);
                        #endregion blank

                        Paragraph paragraph4102 = new Paragraph();
                        paragraph4102.Add(new Chunk(section10.additionalinformation.ToString(), FontFactory.GetFont("Calibri", 15, Font.NORMAL, BaseColor.BLACK)));
                        cell = new PdfPCell(paragraph4102);
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        cell.Padding = 0;
                        cell.PaddingTop = 12; cell.PaddingBottom = 12;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.AddCell(cell);


                        #endregion additionalinfo
                    }
                    #endregion section10

                    //table added to document
                    document.Add(table1);
                    //code to save pdf file in folder 
                    document.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();
                    var filename = Guid.NewGuid();
                    pdfFileName = filename.ToString() + ".pdf";
                    var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName + "/Submit"));
                    if (!exists)
                    {
                        Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName + "/Submit"));
                    }

                    filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName + "/Submit/" + filename + ".pdf");
                    System.IO.File.WriteAllBytes(filePath, bytes);
                }

            }
            return filePath;

        }
        #endregion oldcode

        */

    }
}
