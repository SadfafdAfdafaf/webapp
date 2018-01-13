using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stat.Models
{
    public enum request_type{
        GET,
        CHANGE,
        LOGIN
    }

    public enum server_name{
        GATE,
        AUTH,
        COMP,
        PERS,
        REG
    }

    public class stat
    {
        public int Id { get; set; }

        public server_name server_name { get; set; }

        public request_type request_type { get; set; }

        [Index(IsUnique = true)]
        public Guid state { get; set; }

        public string detail { get; set; }

        public DateTime? Time { get; set; }
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