using System;

namespace WebApplication3.Models
{
    public class ticketmodel
    {
        public decimal ticketid { get; set; }
        public string ticketno { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string business { get; set; }
        public string phoneno { get; set; }
        public string extension { get; set; }
        public string helptopics { get; set; }
        public string issuesummary { get; set; }
        public string details { get; set; }
        public string files { get; set; }
        public string initialstatus { get; set; }
        public DateTime createdate { get; set; }
        public string finalstatus { get; set; }
        public string finaldate { get; set; }
    }
}