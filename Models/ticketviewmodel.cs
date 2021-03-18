using System;
using System.Collections.Generic;

namespace WebApplication3.Models
{
    public class ticketviewmodel
    {
        public decimal ticketid { get; set; }
        public string ticketno { get; set; }
        public string name { get; set; }
        public string email { get; set; }
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
        public DateTime finaldate { get; set; }
        public List<replydetails> replies { get; set; }

    }
    public class replydetails
    {
        public decimal replayid { get; set; }
        public string ticketno { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public string files { get; set; }
        public string status { get; set; }
        public DateTime replydate { get; set; }
    }
}