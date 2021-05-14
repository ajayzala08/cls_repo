using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                string htmlstring = PopulateBody(cfid);
                string filepath = ExportToHtmlPdf(htmlstring);
                dictionary.Add("Filepath", filepath);
                dictionary.Add("CompanyName", companyFolderName);
                pdffomrstatus(cfid, companyFolderName, pdfFileName, filepath, "Submit");
                //response = Request.CreateResponse(HttpStatusCode.OK, Newtonsoft.Json.JsonConvert.SerializeObject(dictionary));
                response = Request.CreateResponse(HttpStatusCode.OK, dictionary);
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Exception");
            }
            return response;
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
            
            string filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + companyFolderName+"/Submit/" + filename + ".pdf");
            System.IO.File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

        private void pdffomrstatus(decimal cfid, string companyname, string filename,string filepath,string status)
        {
            using (var db = new CompanyFormationdbEntities())
            {
                cls_statusmst_tbl tbl = new cls_statusmst_tbl();
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
}
