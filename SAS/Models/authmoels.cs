using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAS.Models
{
    public class authmoels
    {
        public string name { get; set; }
        public string pass { get; set; }
        public int clientid { get; set; }
    }

    public class AuthModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class authcodemoels
    {
        public string grant_type { get; set; }
        public string code { get; set; }
        public string redirect_uri { get; set; }
        public int client_id { get; set; }
    }

    public class authcheckmoels
    {
        public string token { get; set; }
        public string requedrole { get; set; }
    }

    public class MyModel
    {
        public string refreshtoken { get; set; }
    }
}