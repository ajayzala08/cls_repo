using System;

namespace WebApplication3.Models
{
    public class directormodel
    {
        public decimal cfid { get; set; }
        //public string name { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public DateTime dob { get; set; }
        public string occupation { get; set; }
        public string addressline1 { get; set; }
        public string addressline2 { get; set; }
        public string addressline3 { get; set; }
        public string postal { get; set; }
        public string country { get; set; }
        public string nationality { get; set; }
        public string otherdirectorship1 { get; set; }
        public string otherdirectorship2 { get; set; }
        public string otherdirectorship3 { get; set; }
        public string restricted { get; set; }
        public string numberofshare { get; set; }
        public string beneficialowner { get; set; }
    }
}