using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication5.Models
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

    public class LoginViewModel2
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
    }
    public class AuthenticationModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Redirect { get; set; }
        public int ClientId { get; set; }
    }

    public class AuthModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthModel2
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Redirect { get; set; }

    }

    public class authmoels
    {
        public string name { get; set; }
        public string pass { get; set; }
        public int clientid { get; set; }
    }

    public class tokenmessage
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }

        public string token_type { get; set; }
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


    public enum request_type
    {
        GET,
        CHANGE,
        LOGIN
    }

    public enum server_name
    {
        GATE,
        AUTH,
        COMP,
        PERS,
        REG
    }

    public class instatmes
    {
        public int Id { get; set; }

        public server_name server_name { get; set; }

        public request_type request_type { get; set; }

        public Guid state { get; set; }

        public string detail { get; set; }

        public DateTime Time { get; set; }
        public int Np { get; set; }
    }

    public class outstatmes
    {
        public int status { get; set; }
        public string Error { get; set; }

        public instatmes mes { get; set; }
    }

    public class gateinf
    {
        public int anauth { get; set; }
        public int auth { get; set; }

        public List<int> rasp { get; set; }
        public List<int> resp2 { get; set; }

    }

    public class miniinf
    {

        public List<int> rasp { get; set; }
        public List<int> resp2 { get; set; }

    }

    public class statinf
    {
        public miniinf miniinf1 { get; set; }
        public miniinf miniinf2 { get; set; }
        public miniinf miniinf3 { get; set; }

        public gateinf gateinf1 { get; set; }
    }
}