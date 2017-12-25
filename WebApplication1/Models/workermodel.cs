using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class workermodel
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public int Company { get; set; }
        public int Cost { get; set; }
        public int RegionOffice { get; set; }

    }

    public class companiesmodel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CEO { get; set; }
        public int region { get; set; }
    }

    public class personalinfmodel
    {
        public int Id { get; set; }
        public string claster { get; set; }
    }

    public class detailedworkermodel
    {
        public string FIO { get; set; }
        public string Name { get; set; }
        public string CEO { get; set; }
        public int region { get; set; }
        public int Cost { get; set; }
        public int RegionOffice { get; set; }

    }

    public class detailedCEOmodel
    {
        public string Name { get; set; }
        public string CEO { get; set; }
        public string region { get; set; }
        public int Cost { get; set; }
    }

    public class logpair
    {
        public string name { get; set; }
        public string pass { get; set; }
    }

    public class authcodemoels
    {
        public string grant_type { get; set; }
        public string code { get; set; }
        public string redirect_uri { get; set; }
        public int client_id { get; set; }
    }

    public class tokenmessage
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }

        public string token_type { get; set; }
    }

    public class MyModel
    {
        public string refreshtoken { get; set; }
    }
}