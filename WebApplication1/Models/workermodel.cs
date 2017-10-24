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
        public string Company { get; set; }
        public int Cost { get; set; }
        public string RegionOffice { get; set; }
    }
}