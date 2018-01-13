using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;


namespace WebApplication2.Models
{
    public class Worker
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FIO { get; set; }
        public int Company { get; set; }        
        public int Cost { get; set; }
        public int RegionOffice { get; set; }
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

}