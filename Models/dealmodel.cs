using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{

    public class dealmodel
    {
        public string title { get; set; }
        public string state { get; set; }
        public string customValue { get; set; }
		public currencycls currency { get; set; }
		public string company { get; set; }
		public DateTime expectedCloseDate { get; set; }
        public stagecls stage { get; set; }
		public List<string> products { get; set; }
		public List<string> contacts { get; set; }
		public customcls custom { get; set; }
	
    }
	public class currencycls
	{
        public int id { get; set; }
        public string type { get; set; }
    }

	public class stagecls 
	{
        public int id { get; set; }
        public string type { get; set; }
    }

	public class customcls 
	{
	
	}

	public class dealmainmodel
	{
        public string title { get; set; }
        public string state { get; set; }
        public string customValue { get; set; }
        public int currencyId { get; set; }
        public string currencyType { get; set; }
        public string company { get; set; }
        public DateTime expectedCloseDate { get; set; }
        public int stageId { get; set; }
        public string stageType { get; set; }
        public List<string> products { get; set; }
        public List<string> contacts { get; set; }
        public string custom { get; set; }


    }
}