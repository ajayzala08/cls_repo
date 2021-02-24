using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;

namespace WebApplication3.Models
{
    public class secretarymodel
    {
        public decimal cfid { get; set; }
        public string name { get; set; }
        public DateTime dob { get; set; }
        public string addressline1 { get; set; }
        public string addressline2 { get; set; }
        public string addressline3 { get; set; }
        public decimal postal { get; set; }
        public string country { get; set; }
        public string companyname { get; set; }
        public decimal companynumber { get; set; }
        public string companydirector { get; set; }
        public string companyregiseroffice { get; set; }
        public string companyaddressline1 { get; set; }
        public string companyaddressline2 { get; set; }
        public decimal companypostal { get; set; }
        public string companycountry { get; set; }
    }
}