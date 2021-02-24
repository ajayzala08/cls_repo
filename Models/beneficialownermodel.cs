using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class beneficialownermodel
    {
        public decimal cfid { get; set; }
        public string name { get; set; }
        public string addressline1 { get; set; }
        public string addressline2 { get; set; }
        public string addressline3 { get; set; }
        public decimal postalcode { get; set; }
        public string country { get; set; }
        public string nationality { get; set; }
        public string occupation { get; set; }
        public string natureofownership { get; set; }
    }
}